using SimpleTrading.SettingsReader;

namespace Finance.PciDssIntegration.GrpcService
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("PciDssIntegration.DatabaseConnString")]
        public string DatabaseConnString { get; set; }

        [YamlProperty("PciDssIntegration.AuthGrpcServiceUrl")]
        public string AuthGrpcServiceUrl { get; set; }

        [YamlProperty("PciDssIntegration.AuditLogGrpcServiceUrl")]
        public string AuditLogGrpcServiceUrl { get; set; }

        [YamlProperty("PciDssIntegration.SeqUrl")]
        public string SeqUrl { get; set; }

        [YamlProperty("PciDssIntegration.ServiceBusWriter")]
        public string ServiceBusWriter { get; set; }

        //{paymentBridgeGrpcServiceName}_{brandName}@{paymentBridgeGrpcServiceUrl}|{paymentBridgeGrpcServiceName}_{brandName}@{paymentBridgeGrpcServiceUrl} 
        [YamlProperty("PciDssIntegration.PaymentBridgeGrpcServiceMapping")]
        public string PaymentBridgeGrpcServiceMapping { get; set; }
    }
}