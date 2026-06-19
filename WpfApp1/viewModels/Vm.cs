using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Xml.Linq;
using WpfApp1.models;

namespace WpfApp1.viewModels
{
    public class Vm : ObservableObject
    {
        private const string Col_Title_PrefectureName = "D";
        private const string Col_Title_Category1 = "E";
        private const string Col_Title_Category2 = "F";
        private const string Col_Title_Category3 = "G";
        private const string Col_Title_Category4 = "H";
        private const string Col_Title_WindSpeed = "Q";
        private const string Col_Title_SnowfallAmount = "R";
        public RelayCommand OkCommand { get; set; }
        public Vm()
        {
            OkCommand = new RelayCommand(_OkCommand);
        }

        private void _OkCommand()
        {
            var pathData = "D:\\proj\\me\\RevitDev\\WpfApp1\\datas\\data_construction_address.json";
            var path = @"D:\proj\prima\Yamaguchi_Sangyo_Platform\datas\垂直積雪量　一覧-toc_do_gio_muc_do_tuyet.xlsx";
            var sheetName = "積雪・風速元データ";
            if (!File.Exists(path)) throw new Exception("OutputFilePath is not existed");
            var result = new List<ConstructionAddress>();
            try
            {
                using (var wb = OpenWorkbook(path))
                {
                    var ws = wb.Worksheets.FirstOrDefault(s =>
                        string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase));
                    if (ws == null) return;
                    var minR = 8;
                    var maxR = ws.LastRowUsed().RowNumber();
                    for (int i = minR; i < maxR; i++)
                    {
                        var Cell_PrefectureName = ws.Cell(i, Col_Title_PrefectureName);
                        var Cell_Category1 = ws.Cell(i, Col_Title_Category1);
                        var Cell_Category2 = ws.Cell(i, Col_Title_Category2);
                        var Cell_Category3 = ws.Cell(i, Col_Title_Category3);
                        var Cell_Category4 = ws.Cell(i, Col_Title_Category4);
                        var Cell_WindSpeed = ws.Cell(i, Col_Title_WindSpeed);
                        var Cell_SnowfallAmount = ws.Cell(i, Col_Title_SnowfallAmount);
                        var constructionAddress = new ConstructionAddress()
                        {
                            PrefectureName = GetCellText(Cell_PrefectureName),
                            Category1 = GetCellText(Cell_Category1),
                            Category2 = GetCellText(Cell_Category2),
                            Category3 = GetCellText(Cell_Category3),
                            Category4 = GetCellText(Cell_Category4),
                            WindSpeed = GetCellDouble(Cell_WindSpeed),
                            SnowfallAmount = GetCellDouble(Cell_SnowfallAmount),
                        };
                        result.Add(constructionAddress);
                    }
                }
                if (!result.Any()) return;
                File.WriteAllText(pathData, JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static string GetCellText(IXLCell cell)
        {
            return Convert.ToString(cell.CachedValue, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static double GetCellDouble(IXLCell cell)
        {
            var value = cell.CachedValue;
            if (value is double doubleValue)
            {
                return doubleValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            var text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException($"Cannot convert cell {cell.Address} to double.");
            }

            return double.Parse(text, CultureInfo.InvariantCulture);
        }

        private static XLWorkbook OpenWorkbook(string path)
        {
            try
            {
                return new XLWorkbook(path);
            }
            catch (System.Xml.XmlException)
            {
                using (var workbookStream = CreateWorkbookStreamWithoutInvalidRelationships(path))
                {
                    return new XLWorkbook(workbookStream);
                }
            }
        }

        private static MemoryStream CreateWorkbookStreamWithoutInvalidRelationships(string path)
        {
            var output = new MemoryStream();
            using (var input = File.OpenRead(path))
            using (var sourceArchive = new ZipArchive(input, ZipArchiveMode.Read))
            using (var targetArchive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var entry in sourceArchive.Entries)
                {
                    var targetEntry = targetArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                    using (var sourceStream = entry.Open())
                    using (var targetStream = targetEntry.Open())
                    {
                        if (entry.FullName.EndsWith(".rels", StringComparison.OrdinalIgnoreCase))
                        {
                            CopyRelationshipEntryWithoutInvalidItems(sourceStream, targetStream);
                        }
                        else if (string.Equals(entry.FullName, "xl/workbook.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            CopyWorkbookEntryWithoutInvalidItems(sourceStream, targetStream);
                        }
                        else if (entry.FullName.StartsWith("xl/pivotTables/", StringComparison.OrdinalIgnoreCase)
                            && entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            CopyPivotTableEntryWithoutInvalidItems(sourceStream, targetStream);
                        }
                        else
                        {
                            sourceStream.CopyTo(targetStream);
                        }
                    }
                }
            }

            output.Position = 0;
            return output;
        }

        private static void CopyWorkbookEntryWithoutInvalidItems(Stream sourceStream, Stream targetStream)
        {
            var document = XDocument.Load(sourceStream, System.Xml.Linq.LoadOptions.PreserveWhitespace);
            var root = document.Root;
            if (root != null)
            {
                var calculationProperties = root.Elements()
                    .FirstOrDefault(e => string.Equals(e.Name.LocalName, "calcPr", StringComparison.OrdinalIgnoreCase));

                if (calculationProperties != null && calculationProperties.Attribute("calcId") == null)
                {
                    calculationProperties.Remove();
                }
            }

            document.Save(targetStream, System.Xml.Linq.SaveOptions.DisableFormatting);
        }

        private static void CopyPivotTableEntryWithoutInvalidItems(Stream sourceStream, Stream targetStream)
        {
            var document = XDocument.Load(sourceStream, System.Xml.Linq.LoadOptions.PreserveWhitespace);
            var countElementNames = new HashSet<string>
            {
                "pivotFields",
                "items",
                "rowFields",
                "colFields",
                "pageFields",
                "dataFields",
                "rowItems",
                "colItems",
                "formats",
                "conditionalFormats",
                "chartFormats",
                "pivotHierarchies",
            };

            foreach (var element in document.Descendants()
                .Where(e => countElementNames.Contains(e.Name.LocalName) && e.Attribute("count") == null))
            {
                element.SetAttributeValue("count", element.Elements().Count());
            }

            document.Save(targetStream, System.Xml.Linq.SaveOptions.DisableFormatting);
        }

        private static void CopyRelationshipEntryWithoutInvalidItems(Stream sourceStream, Stream targetStream)
        {
            var document = XDocument.Load(sourceStream, System.Xml.Linq.LoadOptions.PreserveWhitespace);
            var root = document.Root;
            if (root != null)
            {
                var invalidRelationships = root.Elements()
                    .Where(e => string.Equals(e.Name.LocalName, "Relationship", StringComparison.OrdinalIgnoreCase)
                        && (e.Attribute("Id") == null || e.Attribute("Type") == null || e.Attribute("Target") == null))
                    .ToList();

                foreach (var relationship in invalidRelationships)
                {
                    relationship.Remove();
                }
            }

            document.Save(targetStream, System.Xml.Linq.SaveOptions.DisableFormatting);
        }
    }
}
