using DotNetCoreDecorators;
using Finance.PciDssIntegration.GrpcService.Domain;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies;
using Finance.PciDssIntegration.GrpcService.PaymentStrategies.Bridges;
using Finance.PciDssIntegration.GrpcService.Postgres;
using Grpc.Net.Client;
using MyCrm.AuditLog.Grpc;
using MyDependencies;
using MyPostgreSQL;
using MyServiceBus.TcpClient;
using ProtoBuf.Grpc.Client;
using Serilog;
using SimpleTrading.Auth.Grpc;
using SimpleTrading.Deposit.Postgresql.Repositories;
using SimpleTrading.MyLogger;
using SimpleTrading.ServiceBus.Contracts;
using SimpleTrading.ServiceBus.PublisherSubscriber.Deposit;
using SimpleTrading.SettingsReader;

namespace Finance.PciDssIntegration.GrpcService
{
    public static class ServiceBinder
    {
        private const string AppName = "PciDssGrpcService";

        private static SettingsModel SettingModel => SettingsReader.ReadSettings<SettingsModel>();
        public static void BindDbRepositories(this IServiceRegistrator sr)
        {
            sr.Register(new DepositRepository(SettingModel.DatabaseConnString, AppName));
            sr.Register<IProviderStrategySettingsRepository>(
                new ProviderStrategySettingsRepository(new PostgresConnection(SettingModel.DatabaseConnString, AppName)));
        }

        public static void BindBridgeServices(this IServiceRegistrator sr)
        {
            ISettingsModelProvider settingsModelProvider = new SettingsModelProvider();
            sr.Register<IPaymentBridgeGrpcServiceManager>(
                new PaymentBridgeGrpcServiceManager(new PaymentBridgeGrpcServiceFactory(),
                    settingsModelProvider));
            sr.Register<IPaymentStrategyManager>(new PaymentStrategyManager());
            sr.Register(settingsModelProvider);
            sr.Register<IPciDssInvoiceFactory>(new PciDssInvoiceFactory());
            sr.Register<IPaymentProcessor>(new PaymentProcessor());
            sr.Register<ICascadingProcessor>(new CascadingProcessor());
            sr.Register<IBridgeSettingsProvider>(new BridgeSettingsProvider());
        }

        public static void BindGrpcServices(this IServiceRegistrator sr)
        {
            sr.Register(
                GrpcChannel.ForAddress(SettingModel.AuthGrpcServiceUrl)
                    .CreateGrpcService<IAuthGrpcService>());

            sr.Register(
                GrpcChannel.ForAddress(SettingModel.AuditLogGrpcServiceUrl)
                    .CreateGrpcService<IMyCrmAuditLogGrpcService>());
        }

        public static ILogger BindSeqLogger(this IServiceRegistrator sr)
        {
            ILogger logger = new MyLogger(AppName, SettingModel.SeqUrl);

            sr.Register(logger);
            Log.Logger = logger;

            return logger;
        }

        public static MyServiceBusTcpClient BindServiceBus(this IServiceRegistrator sr)
        {
            var tcpServiceBus = new MyServiceBusTcpClient(
                () => SettingModel.ServiceBusWriter, AppName);

            var depositCreatePublisher = new DepositCreateMyServiceBusPublisher(tcpServiceBus);
            var depositUpdateStatusPublisher = new DepositStatusUpdateMyServiceBusPublisher(tcpServiceBus);

            sr.Register<IPublisher<DepositCreateServiceBusContract>>(depositCreatePublisher);
            sr.Register<IPublisher<DepositStatusUpdateServiceBusContract>>(depositUpdateStatusPublisher);

            return tcpServiceBus;
        }
    }
}