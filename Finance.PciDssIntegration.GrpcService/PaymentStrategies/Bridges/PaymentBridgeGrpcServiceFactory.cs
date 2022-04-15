namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges
{
    public class PaymentBridgeGrpcServiceFactory : IPaymentBridgeGrpcServiceFactory
    {
        public IPaymentBridgeGrpcService Create(string paymentProviderName, string serviceGrpcUrl)
        {
            return PaymentBridgeGrpcService.Create(paymentProviderName, serviceGrpcUrl);
        }
    }

    public interface IPaymentBridgeGrpcServiceFactory
    {
        IPaymentBridgeGrpcService Create(string paymentProviderName, string serviceGrpcUrl);
    }
}
