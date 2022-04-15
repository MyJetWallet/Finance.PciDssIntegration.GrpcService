using System;
using System.Collections.Generic;
using System.Linq;
using Finance.PciDssIntegration.GrpcService.Domain;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges.Extensions
{
    public static class RandomExtensions
    {
        private static readonly Random Random = new Random();

        public static ProviderStrategySettings GetRandom(this IEnumerable<ProviderStrategySettings> settings)
        {
            var bridgeStrategySettings = settings.ToList();
            var totalWeight = bridgeStrategySettings.Sum(x => x.Weight);
            var randomWeight = Random.Next(1, totalWeight + 1);
            foreach (var bridgeSettings in bridgeStrategySettings)
            {
                if (randomWeight <= bridgeSettings.Weight) return bridgeSettings;

                randomWeight -= bridgeSettings.Weight;
            }

            return default;
        }

        public static ReadOnlySettingsRandomCollection
            AsReadOnlySettingsRandomCollection(this IEnumerable<ProviderStrategySettings> settings)
        {
            return new ReadOnlySettingsRandomCollection(settings.ToList());
        }
    }
}