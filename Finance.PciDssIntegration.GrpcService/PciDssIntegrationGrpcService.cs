using System;
using System.Threading.Tasks;
using Finance.PciDssIntegration.GrpcContracts;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Serilog;

namespace Finance.PciDssIntegration.GrpcService
{
    public class PciDssIntegrationGrpcService : IFinancePciDssIntegrationGrpcService
    {
        private ILogger Logger => ServiceLocator.Logger;

        public ValueTask<GetPaymentSystemResponse> GetActivePaymentSystemAsync()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<MakeDepositResponse> MakeDepositAsync(MakeDepositRequest request)
        {
            Logger.Information("PciDssIntegrationGrpcService start process MakeDepositRequest {@request}", request);
            try
            {
                return await ServiceLocator.PaymentStrategyManager.MakeDepositAsync(request);
            }
            catch (Exception e)
            {
                Logger.Error(e, "MakeDepositAsync failed for request {@request}", request);
                return MakeDepositResponse.Create(null, DepositRequestStatus.ServerError);
            }
        }

        public ValueTask SetActivePaymentSystemAsync(SetPaymentSystemRequest request)
        {
            throw new NotImplementedException();
        }

        public ValueTask<DecodeInfoResponse> DecodeInfoAsync(DecodeInfoRequest request)
        {
            throw new NotImplementedException();
        }
    }
}