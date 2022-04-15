using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Finance.PciDssIntegration.GrpcService.Postgres.Entities
{
    public class PaymentProviderStrategySettingsEntity
    {
        private const char Separator = ',';
        public string PaymentProvider { get; set; }
        public string Brand { get; set; }
        public int Weight { get; set; }
        public string SupportedGeo { get; set; }
        public string RestrictedGeo { get; set; }
        public decimal DepositLimit { get; set; }
        public string TrafficSource { get; set; }
        public string CardScheme { get; set; }

        public PaymentProviderStrategySettingsEntity()
        {
        }

        public PaymentProviderStrategySettingsEntity(string paymentProviderName, string brand, int weight,
            IReadOnlyCollection<string> supportedGeo, IReadOnlyCollection<string> restrictedGeo, 
            decimal depositLimit, string trafficSource, string cardScheme)
        {
            PaymentProvider = paymentProviderName;
            Brand = brand;
            Weight = weight;
            SetSupportedGeo(supportedGeo);
            SetRestrictedGeo(restrictedGeo);
            DepositLimit = depositLimit;
            TrafficSource = trafficSource;
            CardScheme = cardScheme;
        }

        public IReadOnlyCollection<string> GetSupportedGeo()
        {
            if (string.IsNullOrEmpty(SupportedGeo)) return Array.Empty<string>();
            return SupportedGeo.Split(Separator).ToList();
        }

        public IReadOnlyCollection<string> GetRestrictedGeo()
        {
            if (string.IsNullOrEmpty(RestrictedGeo)) return Array.Empty<string>();
            return RestrictedGeo.Split(Separator).ToList();
        }

        public IReadOnlyCollection<string> GetTrafficSource()
        {
            if (string.IsNullOrEmpty(TrafficSource)) return Array.Empty<string>();
            return TrafficSource.Split(Separator).ToList();
        }

        public CardSchemeModel GetCardScheme()
        {
            if (string.IsNullOrEmpty(CardScheme)) return new CardSchemeModel();
            var cards = JsonConvert.DeserializeObject<CardSchemeModel>(CardScheme);
            return cards;
        }

        public void SetSupportedGeo(IReadOnlyCollection<string> supportedGeo)
        {
            SupportedGeo = string.Join(Separator, supportedGeo);
        }

        public void SetRestrictedGeo(IReadOnlyCollection<string> restrictedGeo)
        {
            RestrictedGeo = string.Join(Separator, restrictedGeo);
        }


        public static PaymentProviderStrategySettingsEntity Create(string paymentProviderName, string brand, int weight,
            IReadOnlyCollection<string> supportedGeo, IReadOnlyCollection<string> restrictedGeo,
            decimal depositLimit, string trafficSource, string cardScheme)
        {
            return new PaymentProviderStrategySettingsEntity(paymentProviderName, brand, weight, supportedGeo,
                restrictedGeo, depositLimit, trafficSource, cardScheme);
        }
    }
}
