using System.Collections.Generic;
using System.Threading.Tasks;
using Finance.PciDssIntegration.GrpcService.Postgres;
using Finance.PciDssIntegration.GrpcService.Postgres.Entities;
using MyPostgreSQL;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Finance.PciDssIntegration.GrpcService.Tests
{
    public class PaymentStrategyCardSchemeDatabaseTests
    {
        private readonly ProviderStrategySettingsRepository _settingsRepository;
        private readonly string _connection = "";

        public PaymentStrategyCardSchemeDatabaseTests()
        {
            _settingsRepository = new ProviderStrategySettingsRepository(new PostgresConnection(_connection, "AppName"));
        }

        [Test]
        [Ignore("Cannot mock repository, configure DB connection settings")]
        public async Task Get_All_Entities_From_Database_Async()
        {
            var entities = await _settingsRepository.GetAllAsync();
        }

        [Test]
        public void Parse_Json_Cards_Settings()
        {
            var text = "{\"Visa\":{\"card\":false,\"kyc\":true},\"Mastercard\":{\"card\":true,\"kyc\":false},\"Other\":{\"card\":false,\"kyc\":false}}";
            var cards = JsonConvert.DeserializeObject<CardSchemeModel>(text);
            var result = cards;
            Assert.AreEqual(result.Visa.CardEnabled, false);
            Assert.AreEqual(result.Visa.KycEnabled, true);
            Assert.AreEqual(result.Mastercard.CardEnabled, true);
            Assert.AreEqual(result.Mastercard.KycEnabled, false);
            Assert.AreEqual(result.Other.CardEnabled, false);
            Assert.AreEqual(result.Other.KycEnabled, false);
        }

        [Test]
        public void Create_Json_Cards_Settings()
        {
            var model = new CardSchemeModel();
            model.Visa = new CardModel(){ CardEnabled = false, KycEnabled = true };
            model.Mastercard = new CardModel() { CardEnabled = true, KycEnabled = false };
            model.Other = new CardModel() { CardEnabled = false, KycEnabled = true };
            var json = JsonConvert.SerializeObject(model);
            var text = json.ToString();
            Assert.AreEqual(text,
                "{\"Visa\":{\"card\":false,\"kyc\":true},\"Mastercard\":{\"card\":true,\"kyc\":false},\"Other\":{\"card\":false,\"kyc\":true}}");
        }
    }
}
