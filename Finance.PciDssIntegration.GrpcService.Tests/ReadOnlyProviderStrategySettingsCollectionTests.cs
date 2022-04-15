using System;
using System.Collections.Generic;
using System.Linq;
using Finance.PciDss.Abstractions;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges;
using Finance.PciDssIntegration.GrpcService.Postgres.Entities;
using NUnit.Framework;

namespace Finance.PciDssIntegration.GrpcService.Tests
{
    public class ReadOnlyProviderStrategySettingsCollectionTests
    {
        [Test]
        public void ReadOnlyProviderStrategySettings_ShouldLoopAllElements_Test()
        {
            IReadOnlyCollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("A", "HandelPro", 1, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("B", "HandelPro", 1, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("C", "HandelPro", 1, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("D", "HandelPro", 1, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel())
            };

            var collection = new ReadOnlySettingsRandomCollection(bridgeSettings);
            var count = 0;
            foreach (var item in collection)
            {
                Console.WriteLine(item.PaymentProviderName);
                Assert.IsNotNull(item);
                count++;
            }

            Assert.AreEqual(bridgeSettings.Count, count);
        }

        [Test]
        public void ReadOnlyProviderStrategySettings_ShouldGetDifferentElements_Test()
        {
            IReadOnlyCollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("A", "HandelPro", 1, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("B", "HandelPro", 1, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("C", "HandelPro", 1, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("D", "HandelPro", 1, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
            };

            var collection = new ReadOnlySettingsRandomCollection(bridgeSettings);
            var result = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in collection)
                {
                    result.Add(item.PaymentProviderName);
                    break;
                }
            }

            var group = result.GroupBy(x => x).Select(x=>new { x.Key, count = x.Count() }).ToList();
            foreach (var settings in bridgeSettings)
            {
                Assert.Contains(settings.PaymentProviderName, group.Select(x=>x.Key).ToList());
            }
        }

        [Test]
        public void ReadOnlyProviderStrategySettings_ShouldGetNothing_Test()
        {
            IReadOnlyCollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("A", "HandelPro", 0, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("B", "HandelPro", 0, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("C", "HandelPro", 0, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("D", "HandelPro", 0, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
            };

            var collection = new ReadOnlySettingsRandomCollection(bridgeSettings);
            var result = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in collection)
                {
                    result.Add(item.PaymentProviderName);
                    break;
                }
            }

            Assert.IsEmpty(result);
        }

        [Test]
        public void ReadOnlyProviderStrategySettings_ShouldGetOnlyOneProviderStrategy_WhenProviderStrategySettingsAreDisabled_Test()
        {
            IReadOnlyCollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>
            {
                ProviderStrategySettings.Create("A", "HandelPro", 0, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("B", "HandelPro", 0, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("C", "HandelPro", 0, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
                ProviderStrategySettings.Create("D", "HandelPro", 1, Array.Empty<string>(),
                    Array.Empty<string>(), 0.0m, Array.Empty<string>(), new CardSchemeModel()),
            };

            var collection = new ReadOnlySettingsRandomCollection(bridgeSettings);
            var result = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in collection)
                {
                    result.Add(item.PaymentProviderName);
                    break;
                }
            }

            var group = result.GroupBy(x => x).Select(x => new { x.Key, count = x.Count() }).ToList();
            Assert.AreEqual(1, group.Count);
            Assert.IsTrue(group.Any(x => x.Key.Equals("D")));
        }

        [Test]
        public void ReadOnlyProviderStrategySettings_ShouldNotThrowException__WhenEmpty_Test()
        {
            IReadOnlyCollection<ProviderStrategySettings> bridgeSettings = new List<ProviderStrategySettings>();

            var collection = new ReadOnlySettingsRandomCollection(bridgeSettings);
            var result = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in collection)
                {
                    result.Add(item.PaymentProviderName);
                    break;
                }
            }

            var group = result.GroupBy(x => x).Select(x => new { x.Key, count = x.Count() }).ToList();
            Assert.AreEqual(0, group.Count);
        }
    }
}
