using Prometheus;

namespace Finance.PciDssIntegration.GrpcService
{
    public class MonitoringLocator
    {
        public static readonly Counter Success =
            Metrics.CreateCounter("pci_dss_success_deposit_counter", "Count of success calls to payment providers",
                new CounterConfiguration
                {
                    LabelNames = new[] { "PaymentProvider" }
                });

        public static readonly Counter Failed =
            Metrics.CreateCounter("pci_dss_fail_deposit_counter", "Count of failed calls to payment providers",
                new CounterConfiguration
                {
                    LabelNames = new[] { "PaymentProvider" }
                });

        public static readonly Counter AllFailed =
            Metrics.CreateCounter("pci_dss_fail_deposit_total_counter", "Count of failed calls to all payment providers");
    }
}
