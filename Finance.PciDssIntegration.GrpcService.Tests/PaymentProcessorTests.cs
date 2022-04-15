using System;
using System.Threading.Tasks;
using AutoFixture;
using Finance.PciDss.PciDssBridgeGrpc;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Finance.PciDss.PciDssBridgeGrpc.Contracts.Enums;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges;
using MyDependencies;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using SimpleTrading.Deposit.Postgresql.Models;

namespace Finance.PciDssIntegration.GrpcService.Tests
{
    public class PaymentProcessorTests
    {
        private readonly IServiceResolver _serviceResolver;
        private readonly IPciDssInvoiceFactory _pciDssInvoiceFactory;
        private IPaymentBridgeGrpcServiceManager _paymentBridgeGrpcServiceManager;
        private readonly Fixture _fixture;
        private readonly IFinancePciDssBridgeGrpcService _financePciDssBridgeGrpcService;

        public PaymentProcessorTests()
        {
            _serviceResolver = Substitute.For<IServiceResolver>();
            _pciDssInvoiceFactory = Substitute.For<IPciDssInvoiceFactory>();
            _financePciDssBridgeGrpcService = Substitute.For<IFinancePciDssBridgeGrpcService>();
            _paymentBridgeGrpcServiceManager = Substitute.For<IPaymentBridgeGrpcServiceManager>();
            _serviceResolver.GetService<ILogger>().Returns(Logger.None);
            _serviceResolver.GetService<IPciDssInvoiceFactory>().Returns(_pciDssInvoiceFactory);
            _serviceResolver.GetService<IPaymentBridgeGrpcServiceManager>().Returns(_paymentBridgeGrpcServiceManager);
            _fixture = new Fixture();
            ServiceLocator.Init(_serviceResolver);
        }
        [Ignore("Can not mock deposit repository")]
        [TestCase(DepositBridgeRequestGrpcStatus.Success, DepositRequestStatus.Success)]
        [TestCase(DepositBridgeRequestGrpcStatus.ServerError, DepositRequestStatus.ServerError)]
        public async Task ProcessAsync_ShouldReturnCorrectDepositStatus(
            DepositBridgeRequestGrpcStatus depositBridgeRequestGrpcStatus, DepositRequestStatus depositRequestStatus)
        {
            var request = _fixture.Build<MakeDepositRequest>().With(x => x.Brand, "Monfex").Create<MakeDepositRequest>();
            var depositModel = _fixture.Create<DepositModel>();
            var pciDssInvoice = _fixture.Build<PciDssInvoice>().With(x => x.BrandName, "Monfex").Create<PciDssInvoice>();
            var makeBridgeDepositGrpcResponse = _fixture.Build<MakeBridgeDepositGrpcResponse>()
                .With(x => x.Status, depositBridgeRequestGrpcStatus).Create();
            var providerStrategySettings = _fixture.Build<ProviderStrategySettings>()
                .With(x => x.Weight, 100)
                .Create();
            var paymentProcessor = new PaymentProcessor();
            _financePciDssBridgeGrpcService.MakeDepositAsync(Arg.Any<MakeBridgeDepositGrpcRequest>())
                .Returns(makeBridgeDepositGrpcResponse);
            _paymentBridgeGrpcServiceManager.GetOrCreate(Arg.Any<string>()).FinancePciDssBridgeGrpcService
                .Returns(_financePciDssBridgeGrpcService);
            _pciDssInvoiceFactory.CreatePciDssInvoiceAsync(request, _financePciDssBridgeGrpcService, Arg.Any<string>())
                .Returns(pciDssInvoice);

            var response = await paymentProcessor.ProcessAsync(request, depositModel, providerStrategySettings);
            Assert.AreEqual(depositRequestStatus, response.Status);
        }

        [Test]
        [Ignore("Can not mock deposit repository")]
        public async Task ProcessAsync_ShouldEnrichDepositModel_WhenSuccess()
        {
            var request = _fixture.Build<MakeDepositRequest>().With(x=>x.Brand, "Monfex" ).Create<MakeDepositRequest>();
            var depositModel = _fixture.Create<DepositModel>();
            var pciDssInvoice = _fixture.Build<PciDssInvoice>().With(x => x.BrandName, "Monfex").Create<PciDssInvoice>();
            var makeBridgeDepositGrpcResponse = _fixture.Build<MakeBridgeDepositGrpcResponse>()
                .With(x => x.Status, DepositBridgeRequestGrpcStatus.Success).Create();
            var providerStrategySettings = _fixture.Build<ProviderStrategySettings>()
                .With(x => x.Weight, 100)
                .Create();
            var paymentProcessor = new PaymentProcessor();
            _financePciDssBridgeGrpcService.MakeDepositAsync(Arg.Any<MakeBridgeDepositGrpcRequest>())
                .Returns(makeBridgeDepositGrpcResponse);
            _paymentBridgeGrpcServiceManager.GetOrCreate(Arg.Any<string>()).FinancePciDssBridgeGrpcService
                .Returns(_financePciDssBridgeGrpcService);
            _pciDssInvoiceFactory.CreatePciDssInvoiceAsync(request, _financePciDssBridgeGrpcService, Arg.Any<string>())
                .Returns(pciDssInvoice);

            var response = await paymentProcessor.ProcessAsync(request, depositModel, providerStrategySettings);
            Assert.AreEqual(pciDssInvoice.PsAmount, depositModel.PsAmount);
            Assert.AreEqual(pciDssInvoice.PsCurrency, depositModel.PsCurrency);
            Assert.AreEqual(pciDssInvoice.PaymentProvider, depositModel.PaymentProvider);
            Assert.AreEqual(makeBridgeDepositGrpcResponse.PsTransactionId, depositModel.PsTransactionId);
        }

        [Test]
        public async Task ProcessAsync_ShouldReturnFailedDepositStatusIfThrowException()
        {
            var request = _fixture.Create<MakeDepositRequest>();
            var depositModel = _fixture.Create<DepositModel>();
            var pciDssInvoice = _fixture.Create<PciDssInvoice>();
            var providerStrategySettings = _fixture.Build<ProviderStrategySettings>()
                .With(x => x.Weight, 100)
                .Create();
            var paymentProcessor = new PaymentProcessor();
            _financePciDssBridgeGrpcService.MakeDepositAsync(Arg.Any<MakeBridgeDepositGrpcRequest>())
                .Throws(new Exception());
            _paymentBridgeGrpcServiceManager.GetOrCreate(Arg.Any<string>()).FinancePciDssBridgeGrpcService
                .Returns(_financePciDssBridgeGrpcService);
            _pciDssInvoiceFactory.CreatePciDssInvoiceAsync(request, _financePciDssBridgeGrpcService, Arg.Any<string>())
                .Returns(pciDssInvoice);

            var response = await paymentProcessor.ProcessAsync(request, depositModel, providerStrategySettings);
            Assert.AreEqual(DepositRequestStatus.ServerError, response.Status);
        }
    }
}
