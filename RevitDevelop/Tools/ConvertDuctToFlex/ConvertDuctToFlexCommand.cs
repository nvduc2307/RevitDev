using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;
using Microsoft.Extensions.DependencyInjection;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Tools.ConvertDuctToFlex.action;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.RevCategories;
using RevitDevelop.Utils.RevDuct;
using RevitDevelop.Utils.SelectFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    action.ExcuteOptionA();
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
