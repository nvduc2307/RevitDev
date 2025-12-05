using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.DemoST.CopyFilters.actions;
using RevitDevelop.DemoST.CopyFilters.viewModels;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.DemoST.CopyFilters
{
    [Transaction(TransactionMode.Manual)]
    public class CopyFilterCmd : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var service = new ServiceCollection();
                    service.AddSingleton<CopyFilterCmd>();
                    service.AddSingleton<CopyFilterVm>();
                    service.AddSingleton<CopyFilterAction>();
                    var provider = service.BuildServiceProvider();
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception ex)
                {
                    IO.ShowWarning(ex.Message);
                    tsg.RollBack();
                }
            }

        }
    }
}
