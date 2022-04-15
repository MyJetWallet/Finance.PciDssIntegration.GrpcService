using NUnit.Framework;

namespace Finance.PciDssIntegration.GrpcService.Tests
{
    public class Tests
    {
        [Test]
        public void TestRedirectByAccountId()
        {
            const string mtUrl = "https://personal.handelpro.com";
            const string stUrl = "https://trade.monfex.com/";
            const string defaultRedirectUrl = "http://google.com";
            
            var mappingString = $"st@{stUrl}|mt@{mtUrl}";
            
            var mtLiveInvoice = "mtl1245125124usd".CrateInvoiceByAccount();
            var mtDemoInvoice = "mtd1245125124usd".CrateInvoiceByAccount();
            var stDemoInvoice = "std1245125124usd".CrateInvoiceByAccount();
            var stLiveInvoice = "stl1245125124usd".CrateInvoiceByAccount();
            var trashInvoice = "trashAccount".CrateInvoiceByAccount();

            var mtLiveInvoiceResult = mtLiveInvoice.GetRedirectUrlForInvoice(mappingString, defaultRedirectUrl);
            var stLiveInvoiceResult = stLiveInvoice.GetRedirectUrlForInvoice(mappingString, defaultRedirectUrl);
            var mtDemoInvoiceResult = mtDemoInvoice.GetRedirectUrlForInvoice(mappingString, defaultRedirectUrl);
            var stDemoInvoiceResult = stDemoInvoice.GetRedirectUrlForInvoice(mappingString, defaultRedirectUrl);
            var trashInvoiceResult = trashInvoice.GetRedirectUrlForInvoice(mappingString, defaultRedirectUrl);

            Assert.AreEqual(mtUrl, mtLiveInvoiceResult);
            Assert.AreEqual(mtUrl, mtDemoInvoiceResult);
            Assert.AreEqual(stUrl, stDemoInvoiceResult);
            Assert.AreEqual(stUrl, stLiveInvoiceResult);
            Assert.AreEqual(defaultRedirectUrl, trashInvoiceResult);
        }
    }
}