using Newtonsoft.Json;
using RevitDevelop.Utils.Entities;
using RevitDevelop.Utils.Paths;
using System.IO;

namespace RevitDevelop.Tools.SettingRebars.SettingDiameters.models
{
    public partial class ElementInstances : ObservableObject
    {
        private SettingDiametersCmd _cmd;
        public const string FILE_REBAR_DATABASE_NAME = "RebarDataDiameters.json";
        [ObservableProperty]
        private List<RebarDatabaseInfo> _rebarBarTypeCustoms;
        public SchemaInfo SchemalDataDiameter { get; set; }
        public ElementInstances(SettingDiametersCmd cmd)
        {
            _cmd = cmd;
            SchemalDataDiameter = new SchemaInfo(
                Properties.PropertySchemalInfo.SCHEMAL_REBAR_DIAMETER_GUID,
                Properties.PropertySchemalInfo.SCHEMAL_REBAR_DIAMETER_NAME,
                new SchemaField());
            RebarBarTypeCustoms = GetRebarBarTypeCustom();
        }
        public List<RebarDatabaseInfo> GetRebarBarTypeCustom(bool reset = false)
        {
            var result = new List<RebarDatabaseInfo>();
            try
            {
                var dataDiameterInfo = SchemaInfo.ReadAll(SchemalDataDiameter.SchemaBase, SchemalDataDiameter.SchemaField, _cmd.Document.ProjectInformation);
                var pathRebarDatabaseCustom = $"{PathUtils.RebarDatas}\\{FILE_REBAR_DATABASE_NAME}";
                if (reset)
                    result = JsonConvert.DeserializeObject<List<RebarDatabaseInfo>>(File.ReadAllText(pathRebarDatabaseCustom));
                else
                    result = dataDiameterInfo == null
                            ? JsonConvert.DeserializeObject<List<RebarDatabaseInfo>>(File.ReadAllText(pathRebarDatabaseCustom))
                            : JsonConvert.DeserializeObject<List<RebarDatabaseInfo>>(dataDiameterInfo.Value);
            }
            catch (Exception)
            {
            }
            return result.Where(x => !string.IsNullOrEmpty(x.NameStyle)).OrderBy(x => x.BarDiameter).ToList();
        }
    }
}
