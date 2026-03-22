using System.IO;

namespace RevitDevelop.Utils
{
    public static class FamilyUtils
    {
        public static void LoadFamily(this Document document, List<string> pathFamilies)
        {
            var familySymbols = new FilteredElementCollector(document)
                .WhereElementIsElementType()
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .ToList();
            foreach (var pathFamily in pathFamilies)
            {
                var nameFamily = pathFamily.Split('/').LastOrDefault().Split('.').FirstOrDefault();
                if (familySymbols.Any(x => x.Name.ToUpper() == nameFamily.ToUpper()))
                    continue;
                LoadFamily(document, pathFamily);
            }
        }
        public static void LoadFamily(this Document document, string pathFamily)
        {
            if (!File.Exists(pathFamily))
                return;
            document.LoadFamily(pathFamily, new FamilyLoadOptions(), out Family family);
        }
    }
    public class FamilyLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }
    }
}
