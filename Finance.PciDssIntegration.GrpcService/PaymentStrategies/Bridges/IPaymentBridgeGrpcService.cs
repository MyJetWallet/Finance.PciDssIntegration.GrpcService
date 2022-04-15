using Finance.PciDss.PciDssBridgeGrpc;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges
{
    public interface IPaymentBridgeGrpcService
    {
        public string Name { get; }
        public string ServiceUrl { get; }
        IFinancePciDssBridgeGrpcService FinancePciDssBridgeGrpcService { get; }
        void UpdateUrl(string url);
    }
}