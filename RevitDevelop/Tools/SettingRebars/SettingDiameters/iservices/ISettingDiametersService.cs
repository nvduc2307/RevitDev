using RevitDevelop.Tools.SettingRebars.SettingDiameters.models;

namespace RevitDevelop.Tools.SettingRebars.SettingDiameters.iservices
{
    public interface ISettingDiametersService
    {
        public void CreateDiameterType(List<RebarDatabaseInfo> rebarDatabaseInfos);
    }
}
