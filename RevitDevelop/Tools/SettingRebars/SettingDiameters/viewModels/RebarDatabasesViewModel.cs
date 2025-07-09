using Newtonsoft.Json;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.iservices;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.models;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.services;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.views;
using RevitDevelop.Utils.Directionaries;
using RevitDevelop.Utils.Entities;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.Paths;
using System.IO;

namespace RevitDevelop.Tools.SettingRebars.SettingDiameters.viewModels
{
    public partial class RebarDatabasesViewModel : ObservableObject
    {
        private SettingDiametersCmd _cmd;
        private ISettingDiametersService _settingDiametersService;
        public RebarDatabasesView ViewMain { get; set; }
        public ElementInstances ElementInstances { get; set; }
        public RebarDatabasesViewModel(
            SettingDiametersCmd cmd,
            ElementInstances elementInstances,
            ISettingDiametersService settingDiametersService)
        {
            _cmd = cmd;
            ElementInstances = elementInstances;
            _settingDiametersService = settingDiametersService;
            ViewMain = new RebarDatabasesView() { DataContext = this };
        }
        [RelayCommand]
        private void OK()
        {
            try
            {
                ElementInstances.SchemalDataDiameter.SchemaField.Value = JsonConvert.SerializeObject(ElementInstances.RebarBarTypeCustoms);
                using (var ts = new Transaction(_cmd.Document, "name transaction"))
                {
                    ts.Start();
                    //--------
                    SchemaInfo.Write(ElementInstances.SchemalDataDiameter.SchemaBase, _cmd.Document.ProjectInformation, ElementInstances.SchemalDataDiameter.SchemaField);
                    _settingDiametersService.CreateDiameterType(ElementInstances.RebarBarTypeCustoms);
                    //--------
                    ts.Commit();
                }
                IO.ShowInfo("Completed");
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
            try
            {
                ElementInstances.RebarBarTypeCustoms = ElementInstances.GetRebarBarTypeCustom(true);
            }
            catch (Exception)
            {
            }
        }
    }
}
