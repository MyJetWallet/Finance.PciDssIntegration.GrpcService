using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using SimpleTrading.Abstraction.Payments;
using SimpleTrading.Deposit.Postgresql.Models;
using SimpleTrading.Deposit.Postgresql.Repositories;
using SimpleTrading.ServiceBus.Contracts;

namespace Finance.PciDssIntegration.GrpcService.PaymentStrategies
{
    public class PaymentStrategyManager : IPaymentStrategyManager
    {
        private DepositRepository DepositRepository => ServiceLocator.DepositRepository;
        private ICascadingProcessor CascadingProcessor => ServiceLocator.CascadingProcessor;
        private IPublisher<DepositCreateServiceBusContract> DepositCreatePublisher => ServiceLocator.DepositCreatePublisher;
        private IPublisher<DepositStatusUpdateServiceBusContract> DepositUpdateStatusPublisher => ServiceLocator.DepositUpdateStatusPublisher;

        public async ValueTask<MakeDepositResponse> MakeDepositAsync(MakeDepositRequest makeDepositRequest)
        {
            var depositModel = await CreateDepositTransactionAsync(makeDepositRequest);

            var lastResponse = await CascadingProcessor.CascadingProcessAsync(makeDepositRequest, depositModel);

            if (lastResponse.Status != DepositRequestStatus.Success)
            {
                depositModel.Status = PaymentInvoiceStatusEnum.Failed;

                await depositModel.SendMessageToAuditLog(
                    $"Didn't create invoice request for cart: {makeDepositRequest.BankNumber.MaskString()} for any provider");
            }

            await DepositRepository.Update(depositModel);
            if (depositModel.Status != PaymentInvoiceStatusEnum.Registered)
            {
                await DepositUpdateStatusPublisher.PublishAsync(depositModel.ToDepositStatusUpdateServiceBusContract());
            }

            return lastResponse;
        }

        private async Task<DepositModel> CreateDepositTransactionAsync(MakeDepositRequest makeDepositRequest)
        {
            var orderId = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            var depositModel = makeDepositRequest.ToDepositModel(orderId);
            await DepositRepository.Add(depositModel);
            await DepositCreatePublisher.PublishAsync(depositModel.ToDepositCreateServiceBusContract());
            return depositModel;
        }
    }
}