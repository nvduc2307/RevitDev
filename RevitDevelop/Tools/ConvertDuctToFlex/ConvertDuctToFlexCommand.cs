using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Tools.ConvertDuctToFlex.action;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.Tools.ConvertDuctToFlex
{
    [Transaction(TransactionMode.Manual)]
    public class ConvertDuctToFlexCommand : ExternalCommand
    {
        public override void Execute()
        {
            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var action = new ConvertDuctToFlexAction(UiDocument);
                    action.ExcuteOptionC();
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
