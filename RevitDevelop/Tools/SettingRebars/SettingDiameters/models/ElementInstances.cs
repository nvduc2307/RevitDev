using Newtonsoft.Json;
using RevitDevelop.Utils.Paths;
using System.IO;

namespace RevitDevelop.Tools.SettingRebars.SettingDiameters.models
{
    public partial class ElementInstances : ObservableObject
    {
        public const string FILE_REBAR_DATABASE_NAME = "RebarbarTypeData.json";
        [ObservableProperty]
        private List<RebarDatabaseInfo> _rebarBarTypeCustoms;
        public ElementInstances()
        {
            RebarBarTypeCustoms = GetRebarBarTypeCustom();
        }
        public List<RebarDatabaseInfo> GetRebarBarTypeCustom()
        {
            var result = new List<RebarDatabaseInfo>();
            try
            {
                var pathRebarDatabaseStatic = $"{PathUtils.PathDatas}{FILE_REBAR_DATABASE_NAME}";
                var pathRebarDatabaseCustom = $"{PathUtils.PathDatas}\\{FILE_REBAR_DATABASE_NAME}";
                result = File.Exists(pathRebarDatabaseCustom)
                    ? JsonConvert.DeserializeObject<List<RebarDatabaseInfo>>(File.ReadAllText(pathRebarDatabaseCustom))
                    : !File.Exists(pathRebarDatabaseStatic)
                        ? new List<RebarDatabaseInfo>()
                        : JsonConvert.DeserializeObject<List<RebarDatabaseInfo>>(File.ReadAllText(pathRebarDatabaseStatic));
            }
            catch (Exception)
            {
            }
            return result.Where(x => !string.IsNullOrEmpty(x.NameStyle)).OrderBy(x => x.BarDiameter).ToList();
        }
    }
}
