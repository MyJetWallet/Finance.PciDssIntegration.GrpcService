using System;
using System.Linq;
using System.Threading.Tasks;
using Finance.PciDssIntegration.GrpcService.Abstractions;
using MyCrm.AuditLog.Grpc.Models;
using SimpleTrading.Deposit.Postgresql.Models;

namespace Finance.PciDssIntegration.GrpcService
{
    public static class InvoiceUtils
    {
        public static string GetRedirectUrlForInvoice(this IPciDssInvoiceModel invoice,
            string mappingString, string defaultRedirectUrl)
        {
            var mapping = 
                mappingString
                .Split("|")
                .Select(item => item.Split("@"))
                .ToDictionary(item => item[0], item => item[1]);

            foreach (var (prefix, link) in mapping)
            {
                if (invoice.AccountId.Contains(prefix))
                {
                    return link;
                }
            }

            return defaultRedirectUrl;
        }

        public static async Task SendMessageToAuditLog(this DepositModel depositModel, string message)
        {
            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = depositModel.TraderId,
                Action = "deposit",
                ActionId = depositModel.Id,
                DateTime = DateTime.UtcNow,
                Message = message
            });
        }

        public static async Task SendMessageToAuditLog(this PciDssInvoice pciDssInvoice, string message)
        {
            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pciDssInvoice.TraderId,
                Action = "deposit",
                ActionId = pciDssInvoice.OrderId,
                DateTime = DateTime.UtcNow,
                Message = message
            });
        }
    }
}