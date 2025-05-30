using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;

namespace RevitDevelop.Test
{
    public class TestCmd : ExternalCommand
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
