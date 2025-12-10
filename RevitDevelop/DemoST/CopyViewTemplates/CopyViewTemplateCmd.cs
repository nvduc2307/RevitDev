using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.DemoST.CopyViewTemplates.actions;
using RevitDevelop.DemoST.CopyViewTemplates.viewModels;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.Messages.UI;

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
                    MessageCustom.ShowInfo();
                    IO.ShowInfo("発注専用のダンパファミリのみが処理対象です。ダンパファミリの変数に手動で入力した情報は削除されることがありま す。");
                    //var service = new ServiceCollection();
                    //service.AddSingleton<CopyViewTemplateCmd>();
                    //service.AddSingleton<CopyViewTemplateAction>();
                    //service.AddSingleton<CopyViewTemplateVm>();
                    //var provider = service.BuildServiceProvider();
                    //var vm = provider.GetService<CopyViewTemplateVm>();
                    //vm.MainView.ShowDialog();
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
