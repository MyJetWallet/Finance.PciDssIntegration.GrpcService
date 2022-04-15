using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges.Extensions;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges
{
    public class StrategySettingsEnumerator : IEnumerator<ProviderStrategySettings>
    {
        private IList<ProviderStrategySettings> _providerStrategySettings;
        private IReadOnlyCollection<ProviderStrategySettings> _initProviderStrategySettings;

        public StrategySettingsEnumerator(IReadOnlyCollection<ProviderStrategySettings> providerStrategySettings)
        {
            _initProviderStrategySettings = providerStrategySettings;
            _providerStrategySettings = providerStrategySettings.ToList();
        }

        public bool MoveNext()
        {
            var isEmpty = !_providerStrategySettings.Any();
            if (isEmpty) return false;

            Current = _providerStrategySettings.GetRandom();
            if (Current is { })
            {
                _providerStrategySettings.Remove(Current);
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _providerStrategySettings = _initProviderStrategySettings.ToList();
        }

        public ProviderStrategySettings Current { get; private set; }

        object? IEnumerator.Current => Current;

        public void Dispose()
        {
            _providerStrategySettings.Clear();
            _initProviderStrategySettings = Array.Empty<ProviderStrategySettings>();
        }
    }

    public class ReadOnlySettingsRandomCollection : IReadOnlyCollection<ProviderStrategySettings>
    {
        private readonly List<ProviderStrategySettings> _collection;

        public ReadOnlySettingsRandomCollection(IReadOnlyCollection<ProviderStrategySettings> collection)
        {
            _collection = collection.ToList();
            Count = collection.Count;
        }
        public IEnumerator<ProviderStrategySettings> GetEnumerator()
        {
            return new StrategySettingsEnumerator(_collection);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get; }
    }
}
