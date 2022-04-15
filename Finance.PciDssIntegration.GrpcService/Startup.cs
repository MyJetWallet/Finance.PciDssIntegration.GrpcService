using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDependencies;
using MyServiceBus.TcpClient;
using Prometheus;
using ProtoBuf.Grpc.Server;
using SimpleTrading.BaseMetrics;
using SimpleTrading.ServiceStatusReporterConnector;
using SimpleTrading.SettingsReader;

namespace Finance.PciDssIntegration.GrpcService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private static readonly MyIoc Ioc = new MyIoc();
        private static readonly SettingsModel Settings = SettingsReader.ReadSettings<SettingsModel>();
        private MyServiceBusTcpClient _bus;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            services.AddApplicationInsightsTelemetry(Configuration);

            Ioc.BindDbRepositories();
            Ioc.BindGrpcServices();
            Ioc.BindBridgeServices();
            _bus = Ioc.BindServiceBus();
            var logger = Ioc.BindSeqLogger();
            services.AddCodeFirstGrpc(option =>
            {
                option.BindMetricsInterceptors();
                option.Interceptors.Add<LoggerInterceptor>(logger);
            });

            ServiceLocator.Init(Ioc); 
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.BindIsAlive();
            app.BindServicesTree(Assembly.GetExecutingAssembly());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<PciDssIntegrationGrpcService>();
                endpoints.MapMetrics();
            });

            _bus.Start();
        }
    }
}