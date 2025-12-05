using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.DemoST.CopyViewTemplates.actions;
using RevitDevelop.DemoST.CopyViewTemplates.viewModels;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.DemoST.CopyViewTemplates
{
    [Transaction(TransactionMode.Manual)]
    public class CopyViewTemplateCmd : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var service = new ServiceCollection();
                    service.AddSingleton<CopyViewTemplateCmd>();
                    service.AddSingleton<CopyViewTemplateAction>();
                    service.AddSingleton<CopyViewTemplateVm>();
                    var provider = service.BuildServiceProvider();
                    var vm = provider.GetService<CopyViewTemplateVm>();
                    vm.MainView.ShowDialog();
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
