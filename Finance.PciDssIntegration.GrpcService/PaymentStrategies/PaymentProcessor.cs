using System;
using System.Threading.Tasks;
using Finance.PciDss.PciDssBridgeGrpc;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges;
using Serilog;
using SimpleTrading.Deposit.Postgresql.Models;
using SimpleTrading.Deposit.Postgresql.Repositories;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies
{
    public class PaymentProcessor : IPaymentProcessor
    {
        private IPciDssInvoiceFactory InvoiceFactory => ServiceLocator.PciDssInvoiceFactory;
        private ILogger Logger => ServiceLocator.Logger;
        private DepositRepository DepositRepository => ServiceLocator.DepositRepository;

        private IPaymentBridgeGrpcServiceManager PaymentBridgeGrpcServiceManager =>
            ServiceLocator.PaymentBridgeGrpcServiceManager;

        public async ValueTask<MakeDepositResponse> ProcessAsync(MakeDepositRequest makeDepositRequest,
            DepositModel depositModel, ProviderStrategySettings providerStrategySettings)
        {
            try
            {
                var paymentBridgeGrpcService =
                    PaymentBridgeGrpcServiceManager.GetOrCreate(providerStrategySettings.PaymentProviderNameWithBrand);

                if (paymentBridgeGrpcService != null)
                {
                    
                    var lastPciDssInvoice = await InvoiceFactory.CreatePciDssInvoiceAsync(makeDepositRequest,
                        paymentBridgeGrpcService.FinancePciDssBridgeGrpcService, depositModel.Id);
                    lastPciDssInvoice.EnrichModel(depositModel, null);
                    await DepositRepository.Update(depositModel);

                    var sendPciDssInvoiceResponse =
                        await SendPciDssInvoiceAsync(lastPciDssInvoice,
                            paymentBridgeGrpcService.FinancePciDssBridgeGrpcService);

                    if (sendPciDssInvoiceResponse is null)
                    {
                        await depositModel.SendMessageToAuditLog(
                            $"PaymentProcessor {providerStrategySettings.PaymentProviderName}. Didn't create invoice request for card: {makeDepositRequest.BankNumber.MaskString()}. PsAggregator: ${providerStrategySettings.PaymentProviderName}. sendPciDssInvoiceResponse is null");
                        Logger.Error(
                            "PaymentProcessor {PaymentProviderName}. Didn't create invoice request for traderId {traderId}. sendPciDssInvoiceResponse is null",
                            providerStrategySettings.PaymentProviderName, makeDepositRequest.TraderId);
                    }

                    var lastResponse = sendPciDssInvoiceResponse.ToMakeDepositResponse();
                    var spTransactionId = sendPciDssInvoiceResponse?.PsTransactionId;

                    if (lastResponse.Status == DepositRequestStatus.Success)
                    {
                        lastPciDssInvoice.EnrichModel(depositModel, spTransactionId);
                    }
                    else
                    {
                        await depositModel.SendMessageToAuditLog(
                            $"PaymentProcessor {providerStrategySettings.PaymentProviderName}. Didn't create invoice request for traderId {makeDepositRequest.TraderId} sendPciDssInvoiceResponse with error: {sendPciDssInvoiceResponse?.ErrorMessage}");
                        Logger.Error(
                            "PaymentProcessor {PaymentProviderName}. Didn't create invoice request for traderId {traderId} sendPciDssInvoiceResponse with error: {ErrorMessage}",
                            providerStrategySettings.PaymentProviderName, makeDepositRequest.TraderId,
                            sendPciDssInvoiceResponse?.ErrorMessage);
                    }

                    return lastResponse;
                }

                var message =
                    $"PaymentProcessor {providerStrategySettings.PaymentProviderName}. Not found paymentBridgeGrpcService for brand {makeDepositRequest.Brand}, country {makeDepositRequest.Country},traderId {makeDepositRequest.TraderId} ";
                await depositModel.SendMessageToAuditLog(message);
                Logger.Error(
                    "PaymentProcessor {PaymentProviderName}. Not found paymentBridgeGrpcService for brand {brand}, country {country}, traderId {traderId}. Settings {@settings}",
                    providerStrategySettings.PaymentProviderName, makeDepositRequest.Brand,
                    makeDepositRequest.Country, makeDepositRequest.TraderId,
                    providerStrategySettings);
                return MakeDepositResponse.Create(string.Empty, DepositRequestStatus.ServerError);
            }
            catch (Exception e)
            {
                Logger.Error(e,
                    "PaymentProcessor {PaymentProviderName}. Didn't create invoice request for trader: {traderId}.",
                    providerStrategySettings.PaymentProviderName, makeDepositRequest.TraderId);
                return MakeDepositResponse.Create(string.Empty, DepositRequestStatus.ServerError);
            }
        }


        private async ValueTask<MakeBridgeDepositGrpcResponse>
            SendPciDssInvoiceAsync(
                PciDssInvoice pciDssInvoice,
                IFinancePciDssBridgeGrpcService bridgeGrpcService)
        {
            try
            {
                var responseMakeDeposit =
                    await bridgeGrpcService.MakeDepositAsync(
                        MakeBridgeDepositGrpcRequest.Create(pciDssInvoice.ToGrpc()));

                if (responseMakeDeposit is null)
                {
                    Logger.Error(
                        "PaymentProcessor {PaymentProviderName}. responseMakeDeposit is null. TraderId {traderId}",
                        pciDssInvoice.PaymentProvider, pciDssInvoice.TraderId);
                    await pciDssInvoice.SendMessageToAuditLog(
                        $"PaymentProcessor {pciDssInvoice.PaymentProvider}. responseMakeDeposit is null. TraderId {pciDssInvoice.TraderId}");
                    return null;
                }

                return responseMakeDeposit;
            }
            catch (Exception e)
            {
                Logger.Error(e, "PaymentProcessor {PaymentProvider}. MakeDepositAsync failed. TraderId {traderId}",
                    pciDssInvoice.PaymentProvider, pciDssInvoice.TraderId);
                await pciDssInvoice.SendMessageToAuditLog(
                    $"PaymentProcessor {pciDssInvoice.PaymentProvider}. responseMakeDeposit failed with error {e.Message}. TraderId {pciDssInvoice.TraderId}");
                return null;
            }
        }
    }


    public interface IPaymentProcessor
    {
        ValueTask<MakeDepositResponse> ProcessAsync(MakeDepositRequest makeDepositRequest, DepositModel depositModel,
            ProviderStrategySettings providerStrategySettings);
    }
}