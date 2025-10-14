using RevitDevelop.Utils.RevDuct;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public partial class ConvertDuctToFlexAction : IConvertDuctToFlexAction
    {
        /// <summary>
        /// lấy ra các element là duct, elbow, fitting dạng S
        /// </summary>
        /// <param name="fittingStart"></param>
        /// <param name="fittingEnd"></param>
        /// <returns></returns>
        public List<Element> GetElementsByFittingToFitting(FamilyInstance fittingStart, FamilyInstance fittingEnd)
        {
            var result = new List<Element>();
            var elementsStart = fittingStart.GetElementsByConnector(null, fittingEnd);
            var elementsEnd = fittingEnd.GetElementsByConnector(null, fittingStart);
            foreach (var element in elementsStart)
            {
                if (elementsEnd.Any(x=>x.Id.ToString() == element.Id.ToString()))
                    result.Add(element);
            }
            return result;
        }
    }
}
