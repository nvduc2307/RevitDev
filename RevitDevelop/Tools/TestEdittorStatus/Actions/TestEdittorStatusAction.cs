using Firebase.Database;
using RevitDevelop.Tools.TestEdittorStatus.Models;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Messages;

namespace RevitDevelop.Tools.TestEdittorStatus.Actions
{
    public partial class TestEdittorStatusAction
    {
        
        private TestEdittorStatusCommand _cmd;
        public TestEdittorStatusAction(TestEdittorStatusCommand cmd)
        {
            _cmd = cmd;
        }
        public List<ObjectStatus> GetObjects()
        {
            var results = new List<ObjectStatus>();
            try
            {
                var walls = _cmd.Document.GetElementsFromClass<Wall>(false);
                if (!walls.Any()) throw new Exception();
                foreach (var wall in walls)
                {
                    var tip = WorksharingUtils.GetWorksharingTooltipInfo(_cmd.Document, wall.Id);
                    var obj = new ObjectStatus();
                    obj.Id = long.Parse(wall.Id.ToString());
                    obj.Name = wall.Name;
                    obj.Creator = tip.Creator;
                    obj.Editor = tip.Owner;
                    obj.Comment = wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                    results.Add(obj);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return results;
        }
        public void UpdateObjectStatus(List<ObjectStatus> objectStatuses)
        {
            var idReloads = new List<ElementId>();
            foreach (var objectStatus in objectStatuses)
            {
                var id = new ElementId(objectStatus.Id);
                var tip = WorksharingUtils.GetWorksharingTooltipInfo(_cmd.Document, id);
                if (objectStatus.Editor == tip.Owner)
                    continue;
                objectStatus.Editor = tip.Owner;
                if (tip.Owner == string.Empty)
                {
                    idReloads.Add(id);
                }
            }
        }
    }
}
