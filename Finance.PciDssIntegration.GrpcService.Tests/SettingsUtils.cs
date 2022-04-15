namespace Finance.PciDssIntegration.GrpcService.Tests
{
    public static class SettingsUtils
    {
        public static PciDssInvoice CrateInvoiceByAccount(this string account)
        {
            return new PciDssInvoice
            {
                AccountId = account
            };
        }
    }
}