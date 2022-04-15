using System;
using System.Text.RegularExpressions;
using Destructurama.Attributed;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Finance.PciDss.PciDssBridgeGrpc.Contracts.Enums;
using Finance.PciDss.PciDssBridgeGrpc.Models;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcService.Abstractions;
using SimpleTrading.Abstraction.Payments;
using SimpleTrading.Deposit.Postgresql.Models;
using SimpleTrading.ServiceBus.Contracts;
using BrandName = Finance.PciDssIntegration.GrpcContracts.Contracts.BrandName;

namespace Finance.PciDssIntegration.GrpcService
{
    public class PciDssInvoice : IPciDssInvoiceModel
    {
        [LogMasked(ShowFirst = 6, ShowLast = 4, PreserveLength = true)]
        public string CardNumber { get; set; }
        [NotLogged]
        public string Cvv { get; set; }
        [NotLogged]
        public DateTime ExpirationDate { get; set; }
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        public string FullName { get; set; }
        public double Amount { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string OrderId { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        public string Email { get; set; }
        public string TraderId { get; set; }
        public string AccountId { get; set; }
        public string PaymentProvider { get; set; }
        public string Currency { get; set; }
        public string Ip { get; set; }
        public double PsAmount { get; set; }
        public string PsCurrency { get; set; }
        public string BrandName { get; set; }
        public string PhoneNumber { get; set; }
        public string KycVerified { get; set; }
        public double TotalDeposit { get; set; }
    }    

    public static class ModelUtils
    {
        public static PciDssInvoice ToDomainModel(this MakeDepositRequest request, string id, string email,
            string paymentProvider)
        {
            return new PciDssInvoice
            {
                OrderId = id,
                CardNumber = request.BankNumber,
                Cvv = request.Cvv,
                ExpirationDate = request.ExpirationDate,
                FullName = request.FullName,
                Amount = request.Amount,
                Zip = request.PostalCode,
                Country = request.Country,
                City = request.City,
                Address = request.Address,
                TraderId = request.TraderId,
                AccountId = request.AccountId,
                PaymentProvider = paymentProvider,
                Currency = "USD",
                Email = email,
                Ip = request.ClientIp,
                BrandName = request.Brand,
                PhoneNumber = request.PhoneNumber,
                KycVerified = request.KycVerified,
                TotalDeposit = request.TotalDeposit
    };
        }

        public static DepositModel ToDepositModel(this MakeDepositRequest depositRequest, string orderId)
        {
            return new DepositModel
            {
                Id = orderId,
                PaymentSystem = "BANK_CARDS",
                Currency = "USD",
                Amount = depositRequest.Amount,
                Status = PaymentInvoiceStatusEnum.Registered,
                TraderId = depositRequest.TraderId,
                AccountId = depositRequest.AccountId,
                DateTime = DateTime.UtcNow,
                Brand = depositRequest.Brand.ToPostgre()
            };
        }

        public static void EnrichModel(this IPciDssInvoiceModel invoice, DepositModel depositModel,
            string transactionId = null)
        {
            if (invoice is null) return;
            depositModel.PsTransactionId = transactionId;
            depositModel.PaymentProvider = invoice.PaymentProvider;
            depositModel.PsAmount = invoice.PsAmount;
            depositModel.PsCurrency = invoice.PsCurrency;
        }

        public static PciDssInvoiceGrpcModel ToGrpc(this IPciDssInvoiceModel invoice)
        {
            return new PciDssInvoiceGrpcModel
            {
                OrderId = invoice.OrderId,
                PaymentProvider = invoice.PaymentProvider,
                Amount = invoice.Amount,
                Currency = invoice.Currency,
                Brand = invoice.BrandName.ToPciDss(),
                BrandName = invoice.BrandName,
                AccountId = invoice.AccountId,
                Address = invoice.Address,
                CardNumber = invoice.CardNumber,
                City = invoice.City,
                Country = invoice.Country,
                Cvv = invoice.Cvv,
                Email = invoice.Email,
                ExpirationDate = invoice.ExpirationDate,
                FullName = invoice.FullName,
                Ip = invoice.Ip,
                PsAmount = invoice.PsAmount,
                PsCurrency = invoice.PsCurrency,
                TraderId = invoice.TraderId,
                Zip = invoice.Zip,
                PhoneNumber = invoice.PhoneNumber,
                KycVerification = invoice.KycVerified,
                TotalDeposit = invoice.TotalDeposit
            };
        }

        public static DepositRequestStatus ToDepositRequestStatus(
            this DepositBridgeRequestGrpcStatus bridgeRequestGrpcStatus)
        {
            return bridgeRequestGrpcStatus switch
            {
                DepositBridgeRequestGrpcStatus.Success => DepositRequestStatus.Success,
                DepositBridgeRequestGrpcStatus.InvalidCardNumber => DepositRequestStatus.InvalidCardNumber,
                DepositBridgeRequestGrpcStatus.InvalidCredentials => DepositRequestStatus.InvalidCredentials,
                DepositBridgeRequestGrpcStatus.UnsupportedCardType => DepositRequestStatus.UnsupportedCardType,
                DepositBridgeRequestGrpcStatus.MaxDepositAmountExceeded =>
                    DepositRequestStatus.MaxDepositAmountExceeded,
                DepositBridgeRequestGrpcStatus.MinDepositAmountExceeded =>
                    DepositRequestStatus.MinDepositAmountExceeded,
                DepositBridgeRequestGrpcStatus.PaymentDeclined => DepositRequestStatus.PaymentDeclined,
                DepositBridgeRequestGrpcStatus.ServerError => DepositRequestStatus.ServerError,
                _ => throw new ArgumentOutOfRangeException(nameof(bridgeRequestGrpcStatus), bridgeRequestGrpcStatus,
                    null)
            };
        }

        public static SimpleTrading.Deposit.Postgresql.Models.BrandName ToPostgre(this string brandName)
        {
            if (Enum.TryParse(brandName, out SimpleTrading.Deposit.Postgresql.Models.BrandName brand))
            {
                return brand;
            }
            
            throw new ArgumentOutOfRangeException(nameof(brandName), brandName, null);
        }

        public static PciDss.Abstractions.BrandName ToPciDss(this string brandName)
        {
            if(Enum.TryParse(brandName, out PciDss.Abstractions.BrandName brand))
            {
                return brand;
            }

            throw new ArgumentOutOfRangeException(nameof(brandName), brandName, null);
        }

        public static string MaskString(this string cardNumber)
        {
            var firstDigits = cardNumber.Substring(0, 6);
            var lastDigits = cardNumber.Substring(cardNumber.Length - 4, 4);

            var requiredMask = new string('X', cardNumber.Length - firstDigits.Length - lastDigits.Length);

            var maskedString = string.Concat(firstDigits, requiredMask, lastDigits);
            return Regex.Replace(maskedString, ".{4}", "$0 ");
        }

        public static MakeDepositResponse ToMakeDepositResponse(
            this MakeBridgeDepositGrpcResponse bridgeDepositGrpcResponse)
        {
            if (bridgeDepositGrpcResponse is null)
                return MakeDepositResponse.Create(string.Empty, DepositRequestStatus.ServerError);
            return MakeDepositResponse.Create(bridgeDepositGrpcResponse.SecureRedirectUrl,
                bridgeDepositGrpcResponse.Status.ToDepositRequestStatus());
        }

        public static DepositCreateServiceBusContract ToDepositCreateServiceBusContract(this DepositModel depositModel)
        {
            return new DepositCreateServiceBusContract
            {
                AccountId = depositModel.AccountId,
                Amount = (decimal) depositModel.Amount,
                BrandId = depositModel.Brand.ToString(),
                Currency = depositModel.Currency,
                TraderId = depositModel.TraderId,
                Transactionid = depositModel.TransactionId,
                Status = depositModel.Status.ToString(),
                PaymentSystem = depositModel.PaymentSystem
            };
        }

        public static DepositStatusUpdateServiceBusContract ToDepositStatusUpdateServiceBusContract(this DepositModel depositModel)
        {
            return new DepositStatusUpdateServiceBusContract
            {
                AccountId = depositModel.AccountId,
                Amount = (decimal) depositModel.Amount,
                BrandId = depositModel.Brand.ToString(),
                Currency = depositModel.Currency,
                TraderId = depositModel.TraderId,
                Transactionid = depositModel.TransactionId,
                OldStatus = PaymentInvoiceStatusEnum.Registered.ToString(),
                NewStatus = depositModel.Status.ToString(),
                PaymentSystem = depositModel.PaymentSystem
            };
        }
    }
}