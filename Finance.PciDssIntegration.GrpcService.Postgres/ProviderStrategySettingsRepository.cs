using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finance.PciDss.Abstractions;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.Postgres.Entities;
using MyPostgreSQL;

namespace Finance.PciDssIntegration.GrpcService.Postgres
{
    public class ProviderStrategySettingsRepository : IProviderStrategySettingsRepository
    {
        private readonly IPostgresConnection _postgresConnection;

        public ProviderStrategySettingsRepository(IPostgresConnection postgresConnection)
        {
            _postgresConnection = postgresConnection;
        }

        public async ValueTask<IReadOnlyCollection<ProviderStrategySettings>> GetAllAsync()
        {
            const string sql = "select * from public.paymentproviderstrategysettings_view";
            var entities = await _postgresConnection.GetRecordsAsync<PaymentProviderStrategySettingsEntity>(sql);
            return entities.Select(x => x.MapToDomain()).ToList();
        }

        public async ValueTask<IReadOnlyCollection<ProviderStrategySettings>> GetAsync(string brandName)
        {
            const string sql = "select * from public.paymentproviderstrategysettings_view where brand = @brand";
            var entities = await _postgresConnection.GetRecordsAsync<PaymentProviderStrategySettingsEntity>(sql, new
            {
                brand = brandName
            });
            return entities.Select(x => x.MapToDomain()).ToList();
        }

        public async ValueTask<ProviderStrategySettings> GetAsync(string brandName, string paymentProviderName)
        {
            const string sql =
                "select * from public.paymentproviderstrategysettings_view where brand = @brand and paymentprovider = @paymentprovider";
            var entities =
                await _postgresConnection.GetFirstRecordOrNullAsync<PaymentProviderStrategySettingsEntity>(sql,
                    new
                    {
                        brand = brandName,
                        paymentprovider = paymentProviderName
                    });
            return entities.MapToDomain();
        }
    }
}