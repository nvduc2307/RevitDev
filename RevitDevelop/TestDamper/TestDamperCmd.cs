using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.TestDamper.actions;
using RevitDevelop.TestDamper.viewModels;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.TestDamper
{
    [Transaction(TransactionMode.Manual)]

    public class TestDamperCmd : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var service = new ServiceCollection();
                    service.AddSingleton<TestDamperCmd>();
                    service.AddSingleton<TestDamperAction>();
                    service.AddSingleton<TestDamperVM>();
                    var provider = service.BuildServiceProvider();
                    var vm = provider.GetService<TestDamperVM>();
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
