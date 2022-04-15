using System.Linq;
using System.Threading.Tasks;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges.Extensions;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges
{
    public class BridgeSettingsProvider : IBridgeSettingsProvider
    {
        private IProviderStrategySettingsRepository BridgeSettingsRepository => ServiceLocator.BridgeSettingsRepository;

        public async ValueTask<ReadOnlySettingsRandomCollection> GetAsync(MakeDepositRequest makeDepositRequest)
        {
            var settings = await BridgeSettingsRepository.GetAsync(makeDepositRequest.Brand);
            return settings.Where(x => x.Weight > 0 && x.IsSupportCountry(makeDepositRequest.Country) && x.IsNotRestrictedCountry(makeDepositRequest.Country)
            && x.IsPaymentTypeEnabled(makeDepositRequest.BankNumber) && x.IsKycNeeded(makeDepositRequest.BankNumber, makeDepositRequest.KycVerified)
            && x.IsLimitNotReached(makeDepositRequest.Amount) && x.IsTrafficSourceCompartible(makeDepositRequest.Source)).AsReadOnlySettingsRandomCollection();
        }
    }

    public interface IBridgeSettingsProvider
    {
        ValueTask<ReadOnlySettingsRandomCollection> GetAsync(MakeDepositRequest makeDepositRequest);
    }
}
