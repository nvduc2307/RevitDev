using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.DemoST.CopyViewTemplates
{
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
