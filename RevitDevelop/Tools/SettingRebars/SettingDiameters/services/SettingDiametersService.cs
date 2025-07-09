using Autodesk.Revit.DB.Structure;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.iservices;
using RevitDevelop.Tools.SettingRebars.SettingDiameters.models;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.NumberUtils;

namespace RevitDevelop.Tools.SettingRebars.SettingDiameters.services
{
    public class SettingDiametersService : ISettingDiametersService
    {
        private SettingDiametersCmd _cmd;
        public SettingDiametersService(SettingDiametersCmd cmd)
        {
            _cmd = cmd;
        }
        public void CreateDiameterType(List<RebarDatabaseInfo> rebarDatabaseInfos)
        {
            var diameters = _cmd.Document.GetElementsFromClass<RebarBarType>();
            try
            {
                foreach (var data in rebarDatabaseInfos)
                {
                    try
                    {
                        var dataExisted = diameters.FirstOrDefault(x=>x.Name == data.NameStyle);
                        RebarBarType diaType = null;
                        if (dataExisted == null)
                            diaType = RebarBarType.Create(_cmd.Document);
                        else
                            diaType = dataExisted;
                        diaType.Name = data.NameStyle;
                        var modelDiameter = Math.Round(diaType.BarModelDiameter.FootToMm(), 0);
                        var diameter = Math.Round(diaType.BarNominalDiameter.FootToMm(), 0);
                        var standardBend = Math.Round(diaType.StandardBendDiameter.FootToMm(), 0);
                        var standardHookBend = Math.Round(diaType.StandardHookBendDiameter.FootToMm(), 0);
                        var stirrupHookBend = Math.Round(diaType.StirrupTieBendDiameter.FootToMm(), 0);
                        var maxBend = Math.Round(diaType.MaximumBendRadius.FootToMm(), 0);
                        if (modelDiameter != data.ModelBarDiameter)
                            diaType.BarModelDiameter = data.ModelBarDiameter.MmToFoot();
                        if (diameter != data.BarDiameter)
                            diaType.BarNominalDiameter = data.BarDiameter.MmToFoot();
                        if (standardBend != data.StandardBendDiameter)
                            diaType.StandardBendDiameter = data.StandardBendDiameter.MmToFoot();
                        if (standardHookBend != data.StandardHookBendDiameter)
                            diaType.StandardHookBendDiameter = data.StandardHookBendDiameter.MmToFoot();
                        if (stirrupHookBend != data.StirrupOrTieBendDiameter)
                            diaType.StirrupTieBendDiameter = data.StirrupOrTieBendDiameter.MmToFoot();
                        if (maxBend != data.MaximumBendRadius)
                            diaType.MaximumBendRadius = data.MaximumBendRadius.MmToFoot();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
