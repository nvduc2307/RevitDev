using Newtonsoft.Json;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.models;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.views;
using RevitDevelop.Utils.Directionaries;
using RevitDevelop.Utils.Paths;
using System.IO;

namespace RevitDevelop.Tools.SettingRebars.SettingDiameters.viewModels
{
    public partial class RebarDatabasesViewModel : ObservableObject
    {
        public RebarDatabasesView ViewMain { get; set; }
        public ElementInstances ElementInstances { get; set; }
        public RebarDatabasesViewModel(ElementInstances elementInstances)
        {
            ElementInstances = elementInstances;
            ViewMain = new RebarDatabasesView() { DataContext = this };
        }
        [RelayCommand]
        private void OK()
        {
            try
            {
                var pathRebarDatabaseCustom = $"{PathUtils.PathDatas}\\{ElementInstances.FILE_REBAR_DATABASE_NAME}";
                DirectionaryUtils.CreateDirectory(pathRebarDatabaseCustom);
                var content = JsonConvert.SerializeObject(ElementInstances.RebarBarTypeCustoms);
                File.WriteAllText(pathRebarDatabaseCustom, content);
                ViewMain.Close();
            }
            catch (Exception)
            {
            }
        }
        [RelayCommand]
        private void Cancel()
        {
            ViewMain.Close();
        }
        [RelayCommand]
        private void Reset()
        {
            var pathRebarDatabaseStatic = $"{PathUtils.PathDatas}{ElementInstances.FILE_REBAR_DATABASE_NAME}";

            var dm = File.Exists(pathRebarDatabaseStatic)
            ? JsonConvert.DeserializeObject<List<RebarDatabaseInfo>>(File.ReadAllText(pathRebarDatabaseStatic))
            : ElementInstances.RebarBarTypeCustoms.ToList();
            ElementInstances.RebarBarTypeCustoms = dm.ToList();
        }
    }
}
