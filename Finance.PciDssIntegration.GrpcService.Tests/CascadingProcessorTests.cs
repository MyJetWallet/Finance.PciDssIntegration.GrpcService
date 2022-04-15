using System;
using System.Threading.Tasks;
using AutoFixture;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges;
using Finance.PciDssIntegration.GrpcService.Postgres.Entities;
using MyCrm.AuditLog.Grpc;
using MyDependencies;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using SimpleTrading.Deposit.Postgresql.Models;

namespace Finance.PciDssIntegration.GrpcService.Tests
{
    public class CascadingProcessorTests
    {
        private readonly IServiceResolver _serviceResolver;
        private readonly IBridgeSettingsProvider _bridgeSettingsProvider;
        private readonly IMyCrmAuditLogGrpcService _myCrmAuditLogGrpcService;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly Fixture _fixture;

        public CascadingProcessorTests()
        {
            _serviceResolver = Substitute.For<IServiceResolver>();
            _bridgeSettingsProvider = Substitute.For<IBridgeSettingsProvider>();
            _myCrmAuditLogGrpcService = Substitute.For<IMyCrmAuditLogGrpcService>();
            _paymentProcessor = Substitute.For<IPaymentProcessor>();
            _serviceResolver.GetService<IBridgeSettingsProvider>().Returns(_bridgeSettingsProvider);
            _serviceResolver.GetService<IMyCrmAuditLogGrpcService>().Returns(_myCrmAuditLogGrpcService);
            _serviceResolver.GetService<IPaymentProcessor>().Returns(_paymentProcessor);
            _serviceResolver.GetService<ILogger>().Returns(Logger.None);
            _fixture = new Fixture();
            ServiceLocator.Init(_serviceResolver);
        }

        [Test]
        public async Task CascadingProcessAsync_ShouldReturnSuccess_WhenPaymentProcessorReturnSuccess()
        {
            var cascadingProcessor = new CascadingProcessor();
            var request = _fixture.Create<MakeDepositRequest>();
            var depositModel = _fixture.Create<DepositModel>();
            var providerStrategySettings = _fixture.Build<ProviderStrategySettings>()
                .With(x => x.Weight, 100)
                .Create();
            var failedProviderStrategySettings = _fixture.Build<ProviderStrategySettings>()
                .With(x => x.Weight, 1000000)
                .Create();
            var makeDepositResponse = MakeDepositResponse.Create(string.Empty, DepositRequestStatus.Success);
            var failedMakeDepositResponse =
                MakeDepositResponse.Create(string.Empty, DepositRequestStatus.InvalidCardNumber);
            _paymentProcessor.ProcessAsync(request, depositModel, providerStrategySettings)
                .Returns(makeDepositResponse);
            _paymentProcessor.ProcessAsync(request, depositModel, failedProviderStrategySettings)
                .Returns(failedMakeDepositResponse);
            _bridgeSettingsProvider.GetAsync(request)
                .Returns(new ReadOnlySettingsRandomCollection(new[]
                    {providerStrategySettings, failedProviderStrategySettings}));
            var response = await cascadingProcessor.CascadingProcessAsync(request, depositModel);
            Assert.AreEqual(DepositRequestStatus.Success, response.Status);
        }

        [Test]
        public async Task CascadingProcessAsync_ShouldReturnFailed_WhenAllPaymentProcessorReturnFailed()
        {
            var cascadingProcessor = new CascadingProcessor();
            var request = _fixture.Create<MakeDepositRequest>();
            var depositModel = _fixture.Create<DepositModel>();
            var failedProviderStrategySettings1 = _fixture.Build<ProviderStrategySettings>()
                .With(x => x.Weight, 100)
                .Create();
            var failedProviderStrategySettings2 = _fixture.Create<ProviderStrategySettings>();
            var makeDepositResponse =
                MakeDepositResponse.Create(string.Empty, DepositRequestStatus.InvalidCardNumber);
            var failedMakeDepositResponse =
                MakeDepositResponse.Create(string.Empty, DepositRequestStatus.InvalidCardNumber);
            _paymentProcessor.ProcessAsync(request, depositModel, failedProviderStrategySettings1)
                .Returns(makeDepositResponse);
            _paymentProcessor.ProcessAsync(request, depositModel, failedProviderStrategySettings2)
                .Returns(failedMakeDepositResponse);
            _bridgeSettingsProvider.GetAsync(request)
                .Returns(new ReadOnlySettingsRandomCollection(new[]
                    {failedProviderStrategySettings1, failedProviderStrategySettings2}));
            var response = await cascadingProcessor.CascadingProcessAsync(request, depositModel);
            Assert.AreEqual(DepositRequestStatus.InvalidCardNumber, response.Status);
        }

        [Test]
        public async Task CascadingProcessAsync_ShouldReturnFailed_WhenProviderStrategySettingsNotFound()
        {
            var cascadingProcessor = new CascadingProcessor();
            var request = _fixture.Create<MakeDepositRequest>();
            var depositModel = _fixture.Create<DepositModel>();

            _bridgeSettingsProvider.GetAsync(request)
                .Returns(new ReadOnlySettingsRandomCollection(Array.Empty<ProviderStrategySettings>()));
            var response = await cascadingProcessor.CascadingProcessAsync(request, depositModel);
            Assert.AreEqual(DepositRequestStatus.ServerError, response.Status);
        }

        [TestCase("4390544362820125", "Verified", true, TestName = "Visa_KycVerified")]
        [TestCase("4390544362820125", "NotVerified", true, TestName = "Visa_NotVerified")]
        [TestCase("5555555555554444", "Verified", false, TestName = "Mastercard_KycVerified")]
        [TestCase("5555555555554444", "NotVerified", false, TestName = "Mastercard_NotVerified")]
        [TestCase("3530111333300000", "Verified", true, TestName = "Other_KycVerified")]
        [TestCase("3530111333300000", "NotVerified", false, TestName = "Other_KycVerified")]
        public void PaymentKyc_CascadingProcessAsync_ShouldReturnSuccess_WhenPaymentProcessorReturnSuccess(string cardNumber, string kyc, bool result)
        {
            var cascadingProcessor = new CascadingProcessor();
            var request = _fixture.Create<MakeDepositRequest>();
            var depositModel = _fixture.Create<DepositModel>();
            var providerStrategySettings = _fixture.Build<ProviderStrategySettings>()
                .With(x => x.CardScheme, _fixture.Build<CardSchemeModel>()
                    .With(x => x.Visa, _fixture.Build<CardModel>()
                        .With(x => x.CardEnabled, true)
                        .With(x => x.KycEnabled, false)
                        .Create())
                    .With(x => x.Mastercard, _fixture.Build<CardModel>()
                        .With(x => x.CardEnabled, false)
                        .With(x => x.KycEnabled, true)
                        .Create())
                    .With(x => x.Other, _fixture.Build<CardModel>()
                        .With(x => x.CardEnabled, true)
                        .With(x => x.KycEnabled, true)
                        .Create())
                    .Create())
                .Create();
            // Kyc
            var isKycNeeded = providerStrategySettings.IsKycNeeded(cardNumber, kyc);
            Assert.AreEqual(isKycNeeded, result);
        }

        [TestCase(0, 0, true, TestName = "Limit_Is_Zero")]
        [TestCase(750, 500, true, TestName = "Limit_Is_Less")]
        [TestCase(750, 750, true, TestName = "Limit_Is_Equal")]
        [TestCase(750, 750.01, false, TestName = "Limit_Is_More")]
        public void PaymentLimit_CascadingProcessAsync_ShouldReturnSuccess_WhenPaymentProcessorReturnSuccess(decimal limit, double amount, bool result)
        {
            var cascadingProcessor = new CascadingProcessor();
            var request = _fixture.Create<MakeDepositRequest>();
            var depositModel = _fixture.Create<DepositModel>();
            var providerStrategySettings = _fixture.Build<ProviderStrategySettings>()
                .With(x => x.DepositLimit, limit)
                .With(x => x.CardScheme, _fixture.Build<CardSchemeModel>()
                    .With(x => x.Visa, _fixture.Build<CardModel>()
                        .With(x => x.CardEnabled, true)
                        .With(x => x.KycEnabled, false)
                        .Create())
                    .With(x => x.Mastercard, _fixture.Build<CardModel>()
                        .With(x => x.CardEnabled, false)
                        .With(x => x.KycEnabled, true)
                        .Create())
                    .With(x => x.Other, _fixture.Build<CardModel>()
                        .With(x => x.CardEnabled, true)
                        .With(x => x.KycEnabled, true)
                        .Create())
                    .Create())
                .Create();

            // Limit
            var isLimitNotReached = providerStrategySettings.IsLimitNotReached(amount);
            Assert.AreEqual(isLimitNotReached, result);
        }

        [TestCase("4390544362820125", true, TestName = "Visa_Card")]
        [TestCase("5555555555554444", false, TestName = "Mastercard_KycVerified")]
        [TestCase("3530111333300000", true, TestName = "Other_KycVerified")]
        public void PaymentEnabled_CascadingProcessAsync_ShouldReturnSuccess_WhenPaymentProcessorReturnSuccess(string cardNumber, bool result)
        {
            var cascadingProcessor = new CascadingProcessor();
            var request = _fixture.Create<MakeDepositRequest>();
            var depositModel = _fixture.Create<DepositModel>();
            var providerStrategySettings = _fixture.Build<ProviderStrategySettings>()
                .With(x => x.CardScheme, _fixture.Build<CardSchemeModel>()
                    .With(x => x.Visa, _fixture.Build<CardModel>()
                        .With(x => x.CardEnabled, true)
                        .With(x => x.KycEnabled, false)
                        .Create())
                    .With(x => x.Mastercard, _fixture.Build<CardModel>()
                        .With(x => x.CardEnabled, false)
                        .With(x => x.KycEnabled, true)
                        .Create())
                    .With(x => x.Other, _fixture.Build<CardModel>()
                        .With(x => x.CardEnabled, true)
                        .With(x => x.KycEnabled, true)
                        .Create())
                    .Create())
                .Create();
            //Card
            var isPaymentEnabled = providerStrategySettings.IsPaymentTypeEnabled(cardNumber);
            Assert.AreEqual(isPaymentEnabled, result);
        }
    }
}
