using System;
using System.Threading.Tasks;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges;
using Serilog;
using SimpleTrading.Deposit.Postgresql.Models;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies
{
    public class CascadingProcessor : ICascadingProcessor
    {
        private IBridgeSettingsProvider BridgeSettingsProvider => ServiceLocator.BridgeSettingsProvider;
        private ILogger Logger => ServiceLocator.Logger;
        private IPaymentProcessor PaymentProcessor => ServiceLocator.PaymentProcessor;

        public async ValueTask<MakeDepositResponse> CascadingProcessAsync(MakeDepositRequest makeDepositRequest,
            DepositModel depositModel)
        {
            var settings = await BridgeSettingsProvider.GetAsync(makeDepositRequest);
            if (settings.Count == 0)
            {
                var message =
                    $"CascadingProcessor. Not found payment provider for brand {makeDepositRequest.Brand}, country {makeDepositRequest.Country},traderId {makeDepositRequest.TraderId} ";
                await depositModel.SendMessageToAuditLog(message);
                Logger.Error(
                    "CascadingProcessor. Not found payment provider for brand {brand}, country {country}, traderId {traderId}. Settings {@settings}",
                    makeDepositRequest.Brand, makeDepositRequest.Country, makeDepositRequest.TraderId,
                    settings);
            }

            MakeDepositResponse lastResponse = null;
            foreach (var randomSettings in settings)
                try
                {
                    var message =
                        $"CascadingProcessor. Found payment provider {randomSettings.PaymentProviderName} for brand {makeDepositRequest.Brand}, country {makeDepositRequest.Country},traderId {makeDepositRequest.TraderId} ";
                    await depositModel.SendMessageToAuditLog(message);
                    lastResponse =
                        await PaymentProcessor.ProcessAsync(makeDepositRequest, depositModel, randomSettings);
                    if (lastResponse.Status == DepositRequestStatus.Success)
                    {
                        MonitoringLocator.Success.Labels(randomSettings.PaymentProviderName).Inc();
                        await depositModel.SendMessageToAuditLog(
                            $"CascadingProcessor. Got create invoice request for card: {makeDepositRequest.BankNumber.MaskString()}. PsAggregator: ${depositModel.PaymentProvider}");

                        break;
                    }

                    MonitoringLocator.Failed.Labels(randomSettings.PaymentProviderName).Inc();
                    await depositModel.SendMessageToAuditLog(
                        $"CascadingProcessor. Didn't create invoice request for card: {makeDepositRequest.BankNumber.MaskString()}. PsAggregator: ${randomSettings.PaymentProviderName}");
                    Logger.Error(
                        "CascadingProcessor. Didn't create invoice request for card: {card}. PsAggregator: {PaymentProviderName}",
                        makeDepositRequest.BankNumber.MaskString(),
                        randomSettings.PaymentProviderName);
                }
                catch (Exception e)
                {
                    await depositModel.SendMessageToAuditLog(
                        $"CascadingProcessor. Didn't create invoice request for card: {makeDepositRequest.BankNumber.MaskString()}. PsAggregator: ${randomSettings.PaymentProviderName}");
                    Logger.Error(e, "CascadingProcessor. Didn't create invoice request for trader: {traderId}.",
                        makeDepositRequest.TraderId);
                }

            if (lastResponse?.Status != DepositRequestStatus.Success)
            {
                MonitoringLocator.AllFailed.Inc();
                await depositModel.SendMessageToAuditLog(
                    $"CascadingProcessor. Didn't create invoice request for card: {makeDepositRequest.BankNumber.MaskString()} for any payment provider");
                Logger.Error(
                    "CascadingProcessor. Didn't create invoice request for trader: {traderId} for any payment provider",
                    makeDepositRequest.TraderId);
            }

            var response = lastResponse ?? MakeDepositResponse.Create(string.Empty, DepositRequestStatus.ServerError);
            Logger.Information(
                    "CascadingProcessor. Return response {@response} for trader: {traderId}", response, makeDepositRequest.TraderId);
            return response;
        }
    }

    public interface ICascadingProcessor
    {
        ValueTask<MakeDepositResponse> CascadingProcessAsync(MakeDepositRequest makeDepositRequest,
            DepositModel depositModel);
    }
}