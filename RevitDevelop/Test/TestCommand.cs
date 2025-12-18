using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.Messages;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevCurves;
using RevitDevelop.Utils.SelectFilters;
using RevitDevelop.Utils.SkipWarning;
using System;

namespace RevitDevelop.Test
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : ExternalCommand
    {
        public override void Execute()
        {
            var arr = new List<string>()
            {
                "0_作業_八束",
                "0_作業",
                "0_作業_春日",
                "Z_管理",
                "AM_建築提出用",
                "Z_パブリッシュ用",
            };
            using (var tsg = new TransactionGroup(Document, "new Command"))
            {
                tsg.Start();
                try
                {
                    var bo = BrowserOrganization.GetCurrentBrowserOrganizationForViews(Document);
                    var q = arr.Count;
                    for (var i = 0; i < q - 1; i++)
                    {
                        for (int j = i + 1; j < q; j++)
                        {
                            var compare = Compare(arr[i], arr[j]);
                            if (compare < 0)
                            {
                                var tg = arr[i];
                                arr[i] = arr[j];
                                arr[j] = tg;
                            }
                        }
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
        private int Compare(string x, string y)
        {
            var cx = x.Count();
            var cy = y.Count();
            foreach (var textChar in x)
            {
                var index = x.IndexOf(textChar);
                var num1 = char.ConvertToUtf32(x, index);
                var num2 = char.ConvertToUtf32(y, index);
                if (index == cx - 1)
                    if (num1 == num2) return 1;
                if (index == cy - 1)
                    if (num1 == num2) return -1;
                if (num1 == num2)
                    continue;
                return num1 > num2 ? 1 : -1;
            }
            return 1;
        }
        public class SortString : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var cx = x.Count();
                var cy = y.Count();
                foreach (var textChar in x)
                {
                    var index = x.IndexOf(textChar);
                    var num1 = char.ConvertToUtf32(x, index);
                    var num2 = char.ConvertToUtf32(y, index);
                    if (index == cx - 1)
                        if (num1 == num2) return 1;
                    if (index == cy - 1)
                        if (num1 == num2) return -1;
                    if (num1 == num2)
                        continue;
                    return num1 > num2 ? 1 : -1;
                }
                return 1;
            }
        }
    }
}
