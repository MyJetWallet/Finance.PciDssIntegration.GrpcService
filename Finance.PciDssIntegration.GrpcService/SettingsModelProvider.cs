using SimpleTrading.SettingsReader;

namespace Finance.PciDssIntegration.GrpcService
{
    public class SettingsModelProvider : ISettingsModelProvider
    {
        public SettingsModel Get()
        {
            return SettingsReader.ReadSettings<SettingsModel>();
        }
    }

    public interface ISettingsModelProvider
    {
        SettingsModel Get();
    }
}
