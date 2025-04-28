using RevitDevelop.Utils.FilterElementsInRevit;

namespace RevitDevelop.Utils.RevTags
{
    public class RevTagType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public static List<RevTagType> GetRevTagTypes(Document document, BuiltInCategory builtInCategoryTag)
        {
            try
            {
                var sbs = document.GetElementsFromCategory<FamilySymbol>(builtInCategoryTag);
                return !sbs.Any()
                    ? throw new Exception()
                    : (List<RevTagType>)sbs
                    .Select(x => new RevTagType() { Id = int.Parse(x.Id.ToString()), Name = x.Name })
                    .GroupBy(x => x.Id.ToString())
                    .Select(x => x.FirstOrDefault())
                    .ToList();
            }
            catch (Exception)
            {
            }
            return new List<RevTagType>();
        }
    }
}
