using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.RevPipe;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.SkipWarning;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class Task3Commnad : ExternalCommand
    {
        public override void Execute()
        {

            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var flexPipeTypes = Document.GetElementsFromClass<FlexPipeType>();
                    var pStart = UiDocument.Selection.PickElement(Document, BuiltInCategory.OST_PipeCurves);
                    var pEnd = UiDocument.Selection.PickElement(Document, BuiltInCategory.OST_PipeCurves); ;
                    var ignore = UiDocument.Selection.PickElement(Document, BuiltInCategory.OST_PipeFitting);

                    var gr1s = pStart.GetElementsByConnector(null, ignore);
                    var gr2s = pEnd.GetElementsByConnector(null, ignore);

                    var connector1s = gr1s.SortConnector();
                    var connector2s = gr2s.SortConnector();

                    var geo1 = connector1s.ConvertConnectorToPoint(3, 50);
                    var geo2 = connector2s.ConvertConnectorToPoint(3, 50);

                    UiDocument.Selection.SetElementIds(gr2s.Select(x=>x.Id).ToList());

                    using (var ts = new Transaction(Document, "new transaction"))
                    {
                        ts.SkipAllWarnings();
                        ts.Start();
                        (pStart as Pipe).DuctToFlexPipe(geo1, flexPipeTypes);
                        (pEnd as Pipe).DuctToFlexPipe(geo2, flexPipeTypes);
                        Document.Delete(gr1s.Select(x=>x.Id).ToList());
                        Document.Delete(gr2s.Select(x=>x.Id).ToList());
                        ts.Commit();
                    }

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
