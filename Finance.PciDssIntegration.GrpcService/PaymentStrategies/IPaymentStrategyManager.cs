using System.Threading.Tasks;
using Finance.PciDssIntegration.GrpcContracts.Contracts;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies
{
    public interface IPaymentStrategyManager
    {
        ValueTask<MakeDepositResponse> MakeDepositAsync(MakeDepositRequest makeDepositRequest);
    }
}