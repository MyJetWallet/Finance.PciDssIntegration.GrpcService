using System;
using System.Collections.Generic;
using Finance.PciDss.Abstractions;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges.Extensions;
using Finance.PciDssIntegration.GrpcService.Postgres.Entities;
using NUnit.Framework;

namespace Finance.PciDssIntegration.GrpcService.Tests
{
    public class RandomManagerTests
    {
        [Test]
        public void GetRandom_Should_Return_DifferentBridges()
        {
            ICollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("A", "HandelPro", 25, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("B", "HandelPro", 25, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("C", "HandelPro", 25, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("D", "HandelPro", 25, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel())
            };
            var result = new Dictionary<string, int>();
            for (var i = 0; i < 100; i++)
            {
                var res = bridgeSettings.GetRandom();
                if (result.ContainsKey(res.PaymentProviderName))
                    result[res.PaymentProviderName] = ++result[res.PaymentProviderName];
                else
                    result.Add(res.PaymentProviderName, 1);
            }

            foreach (var (name, count) in result) Assert.AreEqual(25, count, 15);
        }

        [Test]
        public void GetRandom_Should_Return_ProviderStrategySettings_When_Collection_Contains_Only_1_Element()
        {
            ICollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("A", "HandelPro", 25, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel())
            };
            var res = bridgeSettings.GetRandom();

            Assert.IsNotNull(res);
        }

        [Test]
        public void GetRandom_Should_NotReturn_Bridges_When_Weight_0()
        {
            ICollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("A", "HandelPro", 0, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("B", "HandelPro", 25, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("C", "HandelPro", 25, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("D", "HandelPro", 25, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel())
            };
            var result = new Dictionary<string, int>();
            for (var i = 0; i < 1000; i++)
            {
                var res = bridgeSettings.GetRandom();
                if (result.ContainsKey(res.PaymentProviderName))
                    result[res.PaymentProviderName] = ++result[res.PaymentProviderName];
                else
                    result.Add(res.PaymentProviderName, 1);
            }

            Assert.False(result.ContainsKey("A"));
        }

        [Test]
        public void GetRandom_Should_Return_Bridges()
        {
            ICollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("B", "HandelPro", 25, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel())
            };
            var res = bridgeSettings.GetRandom();
            Assert.NotNull(res);
        }
    }
}