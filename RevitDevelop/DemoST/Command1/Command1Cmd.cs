using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.DemoST.Command1.actions;
using RevitDevelop.DemoST.Command1.viewModels;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.DemoST.Command1
{
    [Transaction(TransactionMode.Manual)]
    public class Command1Cmd : ExternalCommand
    {
        public override void Execute()
        {
            try
            {
                var service = new ServiceCollection();
                service.AddSingleton<Command1Cmd>();
                service.AddSingleton<Command1Action>();
                service.AddSingleton<Command1VM>();
                var provider = service.BuildServiceProvider();
                var vm = provider.GetService<Command1VM>();
                vm.MainView.ShowDialog();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
            catch (Exception ex)
            {
                IO.ShowWarning(ex.Message);
            }
        }
    }
}
