using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.DaitoTest.actions;
using RevitDevelop.DaitoTest.Utils;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.SelectFilters;

namespace RevitDevelop.DaitoTest
{
    [Transaction(TransactionMode.Manual)]
    public class DaitoTestCmd : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var clientId = "vJuQwOW3cqU8z7o7LabGRPYiVoRNALlAESCs1mnKDDXy3zYD";
                    var redirect = "http://localhost:8080/api/auth/callback";
                    var t = new Task(async () =>
                    {
                        var res = await acUtils.SignInPkceAsync(clientId, redirect);
                    });
                    t.Start();
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
