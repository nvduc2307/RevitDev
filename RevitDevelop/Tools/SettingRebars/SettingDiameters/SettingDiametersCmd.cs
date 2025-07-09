using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.iservices;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.models;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.services;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.viewModels;

namespace RevitDevelop.Tools.SettingRebars.SettingDiameters
{
    [Transaction(TransactionMode.Manual)]
    public class SettingDiametersCmd : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "name transaction group"))
            {
                tsg.Start();
                try
                {
                    //--------
                    var service = new ServiceCollection();
                    service.AddSingleton<SettingDiametersCmd>();
                    service.AddSingleton<ElementInstances>();
                    service.AddSingleton<RebarDatabasesViewModel>();
                    service.AddSingleton<ISettingDiametersService, SettingDiametersService>();
                    var provider = service.BuildServiceProvider();
                    var viewModel = provider.GetService<RebarDatabasesViewModel>();
                    viewModel.ViewMain.ShowDialog();
                    //--------
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception)
                {
                    tsg.RollBack();
                }
            }
        }
    }
}
