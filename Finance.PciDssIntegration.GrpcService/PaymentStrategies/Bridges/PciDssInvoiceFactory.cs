using System.Threading.Tasks;
using Finance.PciDss.PciDssBridgeGrpc;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using SimpleTrading.Auth.Grpc.Contracts;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges
{
    public class PciDssInvoiceFactory : IPciDssInvoiceFactory
    {
        public async ValueTask<PciDssInvoice> CreatePciDssInvoiceAsync(MakeDepositRequest makeDepositRequest,
            IFinancePciDssBridgeGrpcService bridgeGrpcService, string orderId)
        {
            var getEmailResponse = await ServiceLocator
                .AuthGrpcService
                .GetEmailByIdAsync(new GetEmailByIdGrpcRequest {TraderId = makeDepositRequest.TraderId});

            var paymentSystemName = await bridgeGrpcService.GetPaymentSystemNameAsync();
            var pciDssInvoice =
                makeDepositRequest.ToDomainModel(orderId, getEmailResponse.Email,
                    paymentSystemName.PaymentSystemName);

            await EnrichPaymentProviderAmountAsync(pciDssInvoice, bridgeGrpcService);
            return pciDssInvoice;
        }

        private async ValueTask EnrichPaymentProviderAmountAsync(PciDssInvoice pciDssInvoice,
            IFinancePciDssBridgeGrpcService bridgeGrpcService)
        {
            var paymentSystemCurrency = await bridgeGrpcService.GetPsAmountAsync(
                GetPaymentSystemAmountGrpcRequest.Create(pciDssInvoice.Amount, pciDssInvoice.Currency));
            pciDssInvoice.PsCurrency = paymentSystemCurrency.PaymentSystemCurrency;
            pciDssInvoice.PsAmount = paymentSystemCurrency.PaymentSystemAmount;
        }
    }

    public interface IPciDssInvoiceFactory
    {
        ValueTask<PciDssInvoice> CreatePciDssInvoiceAsync(MakeDepositRequest makeDepositRequest,
            IFinancePciDssBridgeGrpcService bridgeGrpcService, string orderId);
    }
}
