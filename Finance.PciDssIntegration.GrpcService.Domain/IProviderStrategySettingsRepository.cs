using System.Collections.Generic;
using System.Threading.Tasks;
using Finance.PciDss.Abstractions;

namespace Finance.PciDssIntegration.GrpcService.Domain
{
    public interface IProviderStrategySettingsRepository
    {
        ValueTask<IReadOnlyCollection<ProviderStrategySettings>> GetAllAsync();
        ValueTask<IReadOnlyCollection<ProviderStrategySettings>> GetAsync(string brandName);
        ValueTask<ProviderStrategySettings> GetAsync(string brandName, string paymentProviderName);
    }
}