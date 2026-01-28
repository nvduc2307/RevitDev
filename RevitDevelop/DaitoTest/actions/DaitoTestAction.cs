using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using RevitDevelop.DaitoTest.models;
using RevitDevelop.DaitoTest.Utils;

namespace RevitDevelop.DaitoTest.actions
{
    public class DaitoTestAction
    {
        private UIDocument _uidoc;
        public DaitoTestAction(UIDocument uidoc)
        {
            _uidoc = uidoc;
        }
        public void Execute(List<Element> elements)
        {
            var grs = GroupElementOnTree(elements, null);
            var branchs = grs
                .Select(x=> GroupElementInTree(x))
                .Aggregate((a,b)=>a.Concat(b).ToList())
                .ToList();
            if (!branchs.Any())
                return;
            _uidoc.Selection.SetElementIds(branchs.FirstOrDefault().Select(x => x.Id).ToList());
        }
        /// <summary>
        /// Group các element liên kết với nhau bằng connector
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public List<List<Element>> GroupElementOnTree(List<Element> elements, List<BuiltInCategory> categoriesOnTree)
        {
            if (categoriesOnTree == null)
            {
                categoriesOnTree = new List<BuiltInCategory>()
                {
                    BuiltInCategory.OST_DuctCurves,
                    BuiltInCategory.OST_DuctFitting,
                    BuiltInCategory.OST_DuctAccessory,
                    BuiltInCategory.OST_PipeCurves,
                    BuiltInCategory.OST_PipeFitting,
                    BuiltInCategory.OST_PipeAccessory,
                };
            }
            var elementsAfterFilter = elements
                .Where(x=>categoriesOnTree.Any(c=>c == x.Category.BuiltInCategory))
                .ToList();
            var results = new List<List<Element>>();
            if (!elementsAfterFilter.Any()) return results;
            var elementOnTree = new List<Element>() { elementsAfterFilter.FirstOrDefault() };
            var q = elementOnTree.Count;
            for (int i = 0; i < q; i++)
            {
                foreach (var element in elementsAfterFilter)
                {
                    if (element.Id.ToString() == elementOnTree[i].Id.ToString())
                        continue;
                    if (elementOnTree.Any(x => x.Id.ToString() == element.Id.ToString()))
                        continue;
                    var isElementConnect = element.IsElementConnect(elementOnTree[i]);
                    if (isElementConnect)
                    {
                        elementOnTree.Add(element);
                        q = elementOnTree.Count;
                    }
                }
            }
            results.Add(elementOnTree);
            var eles = elementsAfterFilter
                .Where(x => !results.Aggregate((a, b) => a.Concat(b).ToList()).Any(y => y.Id.ToString() == x.Id.ToString()))
                .ToList();
            if (!eles.Any())
                return results;
            else
            {
                var grTrees = GroupElementOnTree(eles, categoriesOnTree);
                results.AddRange(grTrees);
            }
            return results;
        }
        /// <summary>
        /// Group các element trong tree thành các group
        /// Điều kiện tách là khi găp các đối tượng là:
        /// + Fitting không phải là Elbow
        /// + Elbow mà người dùng đã chọn không cần chuyển đổi (type elbow)
        /// + Accessory
        /// </summary>
        public List<List<Element>> GroupElementInTree(
            List<Element> elementsOntree,
            List<BuiltInCategory> categoriesIgnore = null,
            List<PartType> typeFittingsExchange = null,
            List<FamilySymbol> typeElbowsIgnore = null,
            List<BuiltInCategory> cateFittings = null)
        {
            if (categoriesIgnore == null)
            {
                categoriesIgnore = new List<BuiltInCategory>()
                {
                    BuiltInCategory.OST_DuctAccessory,
                    BuiltInCategory.OST_PipeAccessory,
                };
            }
            if(typeFittingsExchange == null)
            {
                typeFittingsExchange = new List<PartType>() { PartType.Elbow, };
            }
            if (cateFittings == null)
            {
                cateFittings = new List<BuiltInCategory>() {
                    BuiltInCategory.OST_DuctFitting,
                    BuiltInCategory.OST_PipeFitting,
                };
            }
            if (typeElbowsIgnore == null)
            {
                typeElbowsIgnore = new List<FamilySymbol>();
            }
            var results = new List<List<Element>>();
            var elementsAfterIgnore = elementsOntree
                .Where(x=> !categoriesIgnore.Any(c=>c == x.Category.BuiltInCategory))
                .Where(x=>
                {
                    if (x is not FamilyInstance fa)
                        return true;
                    if (!cateFittings.Any(x => x == fa.Category.BuiltInCategory))
                        return false;
                    var mep = fa.MEPModel;
                    if (mep == null) return false;
                    if (mep is not MechanicalFitting mechan) return false;
                    if (mechan.PartType != PartType.Elbow) return false;
                    if (typeElbowsIgnore.Any(x => x.Id.ToString() == fa.Symbol.Id.ToString())) return false;
                    return true;
                })
                .ToList();
            if (!elementsAfterIgnore.Any())
                return results;
            var elementOnBranch = new List<Element>() { elementsAfterIgnore.FirstOrDefault() };
            var q = elementOnBranch.Count;
            for (int i = 0; i < q; i++)
            {
                foreach (var element in elementsAfterIgnore)
                {
                    if (element.Id.ToString() == elementOnBranch[i].Id.ToString())
                        continue;
                    if (elementOnBranch.Any(x => x.Id.ToString() == element.Id.ToString()))
                        continue;
                    var isElementConnect = element.IsElementConnect(elementOnBranch[i]);
                    if (isElementConnect)
                    {
                        elementOnBranch.Add(element);
                        q = elementOnBranch.Count;
                    }
                }
            }
            results.Add(elementOnBranch);
            var eles = elementsAfterIgnore
                .Where(x => !results.Aggregate((a, b) => a.Concat(b).ToList()).Any(y => y.Id.ToString() == x.Id.ToString()))
                .ToList();
            if (!eles.Any())
                return results;
            else
            {
                var grBranchs = GroupElementInTree(eles);
                results.AddRange(grBranchs);
            }
            return results;
        }
    }
}
