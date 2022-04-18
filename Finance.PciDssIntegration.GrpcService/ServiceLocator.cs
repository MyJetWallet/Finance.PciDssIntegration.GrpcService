using DotNetCoreDecorators;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges;
using MyCrm.AuditLog.Grpc;
using MyDependencies;
using Serilog;
using SimpleTrading.Auth.Grpc;
using SimpleTrading.Deposit.Postgresql.Repositories;
using SimpleTrading.Payments.ServiceBus.Models;
using SimpleTrading.ServiceBus.Contracts;

namespace Finance.PciDssIntegration.GrpcService
{
    public static class ServiceLocator
    {
        public static ISettingsModelProvider SettingsModelProvider { get; set; }
        public static IBridgeSettingsProvider BridgeSettingsProvider { get; private set; }
        public static ICascadingProcessor CascadingProcessor { get; private set; }
        public static IPaymentProcessor PaymentProcessor { get; private set; }
        public static IPublisher<DepositCreateServiceBusContract> DepositCreatePublisher { get; private set; }
        public static IPublisher<DepositStatusUpdateServiceBusContract> DepositUpdateStatusPublisher { get; private set; }
        public static DepositRepository DepositRepository { get; private set; }
        public static IAuthGrpcService AuthGrpcService { get; private set; }
        public static IMyCrmAuditLogGrpcService AuditLogGrpcService { get; set; }
        public static IPaymentBridgeGrpcServiceManager PaymentBridgeGrpcServiceManager { get; set; }
        public static IPaymentStrategyManager PaymentStrategyManager { get; private set; }
        public static IPciDssInvoiceFactory PciDssInvoiceFactory { get; private set; }
        public static IProviderStrategySettingsRepository BridgeSettingsRepository { get; private set; }
        public static ILogger Logger { get; set; }

        public static void Init(IServiceResolver sr)
        {
            DepositRepository = sr.GetService<DepositRepository>();
            BridgeSettingsRepository = sr.GetService<IProviderStrategySettingsRepository>();
            AuthGrpcService = sr.GetService<IAuthGrpcService>();
            AuditLogGrpcService = sr.GetService<IMyCrmAuditLogGrpcService>();
            Logger = sr.GetService<ILogger>();
            PaymentBridgeGrpcServiceManager = sr.GetService<IPaymentBridgeGrpcServiceManager>();
            PaymentStrategyManager = sr.GetService<IPaymentStrategyManager>();
            PciDssInvoiceFactory = sr.GetService<IPciDssInvoiceFactory>();
            SettingsModelProvider = sr.GetService<ISettingsModelProvider>();
            BridgeSettingsProvider = sr.GetService<IBridgeSettingsProvider>();
            CascadingProcessor = sr.GetService<ICascadingProcessor>();
            PaymentProcessor = sr.GetService<IPaymentProcessor>();
            DepositCreatePublisher = sr.GetService<IPublisher<DepositCreateServiceBusContract>>();
            DepositUpdateStatusPublisher = sr.GetService<IPublisher<DepositStatusUpdateServiceBusContract>>();
        }
    }
}