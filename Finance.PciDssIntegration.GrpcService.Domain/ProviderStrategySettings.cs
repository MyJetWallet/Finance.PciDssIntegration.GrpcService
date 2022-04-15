using System;
using System.Collections.Generic;
using System.Linq;
using Finance.CardValidator;
using Finance.PciDss.Abstractions;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcService.Postgres.Entities;

namespace Finance.PciDssIntegration.GrpcService.Domain
{
    public class ProviderStrategySettings
    {
        public ProviderStrategySettings(string paymentProviderName, string brand, int weight,
            IReadOnlyCollection<string> supportedGeo,
            IReadOnlyCollection<string> restrictedGeo,
            decimal depositLimit,
            IReadOnlyCollection<string> trafficSource,
            CardSchemeModel cardScheme)
        {
            PaymentProviderName = paymentProviderName;
            Brand = brand;
            Weight = weight;
            SupportedGeo = supportedGeo ?? Array.Empty<string>();
            RestrictedGeo = restrictedGeo ?? Array.Empty<string>();
            DepositLimit = depositLimit;
            TrafficSource = trafficSource ?? Array.Empty<string>();
            CardScheme = cardScheme ?? new CardSchemeModel();
        }

        public string PaymentProviderName { get; set; }
        public string PaymentProviderNameWithBrand => $"{PaymentProviderName}_{Brand}";
        public string Brand { get; set; }
        public int Weight { get; set; }
        public IReadOnlyCollection<string> SupportedGeo { get; private set; }
        public IReadOnlyCollection<string> RestrictedGeo { get; private set; }
        public decimal DepositLimit { get; set; }
        public IReadOnlyCollection<string> TrafficSource { get; set; }
        public CardSchemeModel CardScheme { get; set; }

        public void Update(ProviderStrategySettings bridgeStrategySettings)
        {
            RestrictedGeo = bridgeStrategySettings.RestrictedGeo ?? Array.Empty<string>();
            SupportedGeo = bridgeStrategySettings.SupportedGeo ?? Array.Empty<string>();
        }

        public static ProviderStrategySettings Create(string paymentProviderName, string brand, int weight,
            IReadOnlyCollection<string> supportedGeo,
            IReadOnlyCollection<string> restrictedGeo,
            decimal depositLimit,
            IReadOnlyCollection<string> trafficSource,
            CardSchemeModel cardScheme)
        {
            return new ProviderStrategySettings(paymentProviderName, brand, weight, supportedGeo, restrictedGeo,
                depositLimit, trafficSource, cardScheme);
        }

        public bool IsSupportCountry(string country)
        {
            return SupportedGeo.Count == 0 || SupportedGeo.Contains(country, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsNotRestrictedCountry(string country)
        {
            return RestrictedGeo.Count == 0 || !RestrictedGeo.Contains(country, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsPaymentTypeEnabled(string cardNumber)
        {
            if (String.IsNullOrEmpty(cardNumber))
                cardNumber = string.Empty;

            var cardValidator = new CardValidator.CardValidator(cardNumber);

            if (!cardValidator.IsCardValid())
                return false;

            if (cardValidator.GetCardType() == CardType.Visa) 
                return CardScheme.Visa.CardEnabled;

            if (cardValidator.GetCardType() == CardType.MasterCard)
                return  CardScheme.Mastercard.CardEnabled;

            return CardScheme.Other.CardEnabled;
        }

        public bool IsKycNeeded(string cardNumber, string kycVerified)
        {
            if (String.IsNullOrEmpty(kycVerified))
                kycVerified = string.Empty;

            if (String.IsNullOrEmpty(cardNumber))
                cardNumber = string.Empty;

            var cardValidator = new CardValidator.CardValidator(cardNumber);

            if (!cardValidator.IsCardValid())
                return false;

            bool isKycVerified = 
                     string.Equals(kycVerified, "Verified", StringComparison.OrdinalIgnoreCase);

            if (cardValidator.GetCardType() == CardType.Visa)
            {
                if (!CardScheme.Visa.CardEnabled)
                    return false;
                
                if (CardScheme.Visa.KycEnabled && !isKycVerified)
                    return false;

                return true;
            }

            if (cardValidator.GetCardType() == CardType.MasterCard)
            {
                if (!CardScheme.Mastercard.CardEnabled)
                    return false;
                
                if(CardScheme.Mastercard.KycEnabled && !isKycVerified)
                    return false;

                return true;
            }

            if (!CardScheme.Other.CardEnabled)
                return false;

            if (CardScheme.Other.KycEnabled && !isKycVerified)
                return false;

            return true;
        }

        public bool IsLimitNotReached(double amount)
        {
            if (DepositLimit == 0.0m)
                return true;

            return Convert.ToDecimal(amount) <= DepositLimit;
        }
        public bool IsTrafficSourceCompartible(string source)
        {
            return TrafficSource.Count == 0 || TrafficSource.Contains(source, StringComparer.OrdinalIgnoreCase);
        }
        
    }
}