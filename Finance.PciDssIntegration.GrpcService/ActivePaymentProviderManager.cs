using Finance.PciDssIntegration.GrpcService.Abstractions;

namespace Finance.PciDssIntegration.GrpcService
{
    public static class ActivePaymentProviderManager
    {
        private static readonly object _lockObject = new object();

        private static DomainPaymentProvider CurrentProvider { get; set; }

        public static void SetActivePaymentProvider(DomainPaymentProvider provider)
        {
            lock (_lockObject)
            {
                CurrentProvider = provider;
            }
        }

        public static DomainPaymentProvider GetActivePaymentProvider()
        {
            return CurrentProvider;
        }
    }
}