using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.DemoST.Command3.actions;
using RevitDevelop.DemoST.Command3.viewModels;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.DemoST.Command3
{
    [Transaction(TransactionMode.Manual)]
    public class Command3Cmd : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var service = new ServiceCollection();
                    service.AddSingleton<Command3Cmd>();
                    service.AddSingleton<Command3Action>();
                    service.AddSingleton<Command3VM>();
                    var provider = service.BuildServiceProvider();
                    var vm = provider.GetService<Command3VM>();
                    vm.MainView.Show();
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
