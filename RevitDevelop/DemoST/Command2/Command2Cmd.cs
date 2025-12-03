using Autodesk.Revit.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.DemoST.Command3;
using RevitDevelop.Utils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.DemoST.Command2
{
    [Transaction(TransactionMode.Manual)]
    public class Command2Cmd : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var service = new ServiceCollection();
                    service.AddSingleton<Command2Cmd>();
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
