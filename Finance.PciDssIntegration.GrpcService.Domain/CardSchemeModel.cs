using Newtonsoft.Json;

namespace Finance.PciDssIntegration.GrpcService.Postgres.Entities
{
    public class CardModel
    {
        [JsonProperty("card")]
        public bool CardEnabled { get; set; }
        [JsonProperty("kyc")]
        public bool KycEnabled { get; set; }

        public CardModel()
        {
            CardEnabled = false;
            KycEnabled = false;
        }
    }

    public class CardSchemeModel
    {
        public CardModel Visa { get; set; }
        public CardModel Mastercard { get; set; }
        public CardModel Other { get; set; }

        public CardSchemeModel()
        {
            Visa = new CardModel();
            Mastercard = new CardModel();
            Other = new CardModel();
        }
    }
}
