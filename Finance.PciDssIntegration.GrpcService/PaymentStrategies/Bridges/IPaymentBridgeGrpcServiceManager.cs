using System.Collections.Generic;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges
{
    public interface IPaymentBridgeGrpcServiceManager
    {
        IPaymentBridgeGrpcService GetOrCreate(string paymentProviderName);
        IReadOnlyCollection<IPaymentBridgeGrpcService> GetAll();
        void Reload();
    }
}