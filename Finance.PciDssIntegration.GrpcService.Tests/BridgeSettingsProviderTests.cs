using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges;
using MyDependencies;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finance.PciDssIntegration.GrpcService.Postgres.Entities;

namespace Finance.PciDssIntegration.GrpcService.Tests
{
    public class BridgeSettingsProviderTests
    {
        private IServiceResolver _serviceResolver;
        private IProviderStrategySettingsRepository _repository;
        private CardSchemeModel _card;

        public BridgeSettingsProviderTests()
        {
            _serviceResolver = Substitute.For<IServiceResolver>();
            _repository = Substitute.For<IProviderStrategySettingsRepository>();
            _serviceResolver.GetService<IProviderStrategySettingsRepository>().Returns(_repository);
            ServiceLocator.Init(_serviceResolver);
            _card = new CardSchemeModel {Other = {CardEnabled = true}};
        }

        [Test]
        public async Task GetAsync_ShouldReturnSettings_WhenCountryIsSupported()
        {
            IReadOnlyCollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("A", "HandelPro", 1, new List<string>(){"UK" },
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), _card),
                ProviderStrategySettings.Create("B", "HandelPro", 1, new List<string>(){"RU" },
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), _card),
                ProviderStrategySettings.Create("C", "HandelPro", 1, new List<string>(){"ZH" },
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), _card),
                ProviderStrategySettings.Create("D", "HandelPro", 1, new List<string>(){"UA" },
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), _card)
            };
            var bridgeSettingsProvider = new BridgeSettingsProvider();
            _repository.GetAsync(Arg.Any<string>()).Returns(bridgeSettings);
            var request = new MakeDepositRequest()
            {
                Country = "UA"
            };
            var result = await bridgeSettingsProvider.GetAsync(request);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public async Task GetAsync_ShouldReturnNothingSettings_WhenCountryIsRestricted()
        {
            IReadOnlyCollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("A", "HandelPro", 1, new List<string>(){"UK" }, Array.Empty<string>(),
                    0.0m, Array.Empty<string>(), _card),
                ProviderStrategySettings.Create("B", "HandelPro", 1, new List<string>(){"RU" }, Array.Empty<string>(), 
                    0.0m, Array.Empty<string>(), _card),
                ProviderStrategySettings.Create("C", "HandelPro", 1, Array.Empty<string>(), new List<string>(){"UA" },
                    0.0m, Array.Empty<string>(), _card),
                ProviderStrategySettings.Create("D", "HandelPro", 1, new List<string>(){"UA" }, new List<string>(){"UA" }, 
                    0.0m, Array.Empty<string>(), _card)
            };
            var bridgeSettingsProvider = new BridgeSettingsProvider();
            _repository.GetAsync(Arg.Any<string>()).Returns(bridgeSettings);
            var request = new MakeDepositRequest()
            {
                Country = "UA"
            };
            var result = await bridgeSettingsProvider.GetAsync(request);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public async Task GetAsync_ShouldReturnSettings_WhenAllCountriesAreSupported()
        {
            //CardSchemeModel card = new CardSchemeModel {Visa = {CardEnabled = true}};
            IReadOnlyCollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("A", "HandelPro", 1, new List<string>(){"UK" },
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), _card),
                ProviderStrategySettings.Create("B", "HandelPro", 1, new List<string>(){"RU" },
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), _card),
                ProviderStrategySettings.Create("C", "HandelPro", 1, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), _card),
                ProviderStrategySettings.Create("D", "HandelPro", 1, new List<string>(){"UA" },
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), _card)
            };
            var bridgeSettingsProvider = new BridgeSettingsProvider();
            _repository.GetAsync(Arg.Any<string>()).Returns(bridgeSettings);
            var request = new MakeDepositRequest()
            {
                Country = "UA"
            };
            var result = await bridgeSettingsProvider.GetAsync(request);
            Assert.AreEqual(2, result.Count);
        }
    }
}
