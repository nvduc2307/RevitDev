using Autodesk.Revit.DB.Mechanical;
using RevitDevelop.Utils.RevCategories;

namespace RevitDevelop.Tools.ConvertDuctToFlex.action
{
    public partial class ConvertDuctToFlexAction
    {
        private void _validateElement(List<Element> elements, out List<Element> elementsInValid)
        {
            //valid element is Duct or ELbow or Offset type S
            elementsInValid = new List<Element>();
            foreach (Element element in elements)
            {
                var cate = element.Category.ToBuiltinCategory();
                if (cate == BuiltInCategory.OST_DuctCurves)
                    continue;
                if (cate == BuiltInCategory.OST_DuctFitting)
                {
                    var fitting = element as FamilyInstance;
                    if (fitting == null)
                    {
                        elementsInValid.Add(element);
                        continue;
                    }
                    var mepModel = fitting.MEPModel as MechanicalFitting;
                    if (mepModel == null)
                    {
                        elementsInValid.Add(element);
                        continue;
                    }
                    if (mepModel.PartType == PartType.Elbow)
                        continue;
                    if (mepModel.PartType == PartType.Offset)
                        continue;
                }
                elementsInValid.Add(element);
            }
        }
    }
}
