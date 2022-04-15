using System;
using Finance.PciDss.Abstractions;
using Finance.PciDssIntegration.GrpcService.Domain;

namespace Finance.PciDssIntegration.GrpcService.Postgres.Entities
{
    public static class PaymentProviderStrategySettingsEntityExtensions
    {
        public static ProviderStrategySettings MapToDomain(this PaymentProviderStrategySettingsEntity entity)
        {
            if (entity is null) return default;
            return ProviderStrategySettings.Create(entity.PaymentProvider, entity.Brand, entity.Weight,
                entity.GetSupportedGeo(), entity.GetRestrictedGeo(), 
                entity.DepositLimit, entity.GetTrafficSource(), entity.GetCardScheme());
        }
    }
}