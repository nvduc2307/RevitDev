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
                    var ac = Application.LoginUserId;
                    var us = Application.Username;
                    //var action = new DaitoTestAction(UiDocument);
                    //var eles = UiDocument.Selection.PickElements(Document);
                    //action.Execute(eles);
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
