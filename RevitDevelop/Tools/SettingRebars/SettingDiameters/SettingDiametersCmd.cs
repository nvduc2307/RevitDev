using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;

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
                    var provider = service.BuildServiceProvider();
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
