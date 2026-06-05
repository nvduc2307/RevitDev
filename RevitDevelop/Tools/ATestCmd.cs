using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitDevelop.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace RevitDevelop.Tools
{
    [Transaction(TransactionMode.Manual)]
    public class ATestCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            var result = Result.Succeeded;
            var uiDocument = commandData.Application.ActiveUIDocument;
            var document = uiDocument.Document;
            using (var tsg = new TransactionGroup(document, "Command"))
            {
                tsg.Start();
                try
                {
                    var nameSchedule = "配管継手集計エルボ樹脂管以外1";
                    var schedule = new FilteredElementCollector(document)
                        .WhereElementIsNotElementType()
                        .OfClass(typeof(ViewSchedule))
                        .Cast<ViewSchedule>()
                        .FirstOrDefault(x => x.Name == nameSchedule);
                    if (schedule == null) throw new Exception();

                    TableData tableData = schedule.GetTableData();
                    TableSectionData body = tableData.GetSectionData(SectionType.Body);
                    int nRows = body.NumberOfRows;
                    int nCols = body.NumberOfColumns;
                    var datas = new List<string>();
                    
                    for (int r = 0; r < nRows; r++)
                    {
                        string row = "";

                        for (int c = 0; c < nCols; c++)
                        {
                            string text = schedule.GetCellText(SectionType.Body, r, c);
                            row += text + " | ";
                        }
                        datas.Add(row);
                    }
                    tsg.Assimilate();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                catch (Exception ex)
                {
                    IO.ShowWarning(ex.Message);
                    tsg.RollBack();
                    result = Result.Failed;
                }
            }
            return result;

        }
    }
}
