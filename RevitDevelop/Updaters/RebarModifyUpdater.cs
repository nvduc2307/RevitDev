using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using RevitDevelop.Utils.BoundingBoxs;
using RevitDevelop.Utils.Entities;
using RevitDevelop.Utils.RevParameters;
using RevitDevelop.Utils.RevRebars;

namespace RevitDevelop.Updaters
{
    public class RebarModifyUpdater : IUpdater
    {
        static AddInId m_appId;
        static UpdaterId m_updaterId;
        private SchemaInfo m_schemaInfoRebar;
        private SchemaInfo m_schemaInfoLap;
        public RebarModifyUpdater(AddInId id)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, new Guid("56644677-e4f5-44fd-a89d-ed3e8b372afa"));
            m_schemaInfoRebar = new SchemaInfo(
                Properties.PropertySchemalInfo.SCHEMAL_REBAR_LAP_GUID,
                Properties.PropertySchemalInfo.SCHEMAL_REBAR_LAP_NAME,
                new SchemaField());
            m_schemaInfoLap = new SchemaInfo(
                Properties.PropertySchemalInfo.SCHEMAL_LAP_GUID,
                Properties.PropertySchemalInfo.SCHEMAL_LAP_NAME,
                new SchemaField());
        }
        public void Execute(UpdaterData data)
        {
            try
            {
                Document doc = data.GetDocument();
                //
                foreach (ElementId addedElemId in data.GetModifiedElementIds())
                {
                    try
                    {
                        var rb = doc.GetElement(addedElemId) as Rebar;
                        if (rb == null)
                            continue;
                        var rbLapInfo = RevRebarLap.GetRevRebarLap(rb);
                        var rebarLapSchemaInfo = SchemaInfo.ReadAll(m_schemaInfoRebar.SchemaBase, m_schemaInfoRebar.SchemaField, rb);
                        if (rebarLapSchemaInfo == null)
                        {
                            if (rbLapInfo.LapWeldStart)
                            {
                                rbLapInfo.LapCouplerStart = false;
                                rbLapInfo.LapPlateStart = false;
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_START, rbLapInfo.LapCouplerStart ? "1" : "0");
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_START, rbLapInfo.LapPlateStart ? "1" : "0");
                            }
                            if (rbLapInfo.LapCouplerStart)
                            {
                                rbLapInfo.LapWeldStart = false;
                                rbLapInfo.LapPlateStart = false;
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_START, rbLapInfo.LapWeldStart ? "1" : "0");
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_START, rbLapInfo.LapPlateStart ? "1" : "0");
                            }
                            if (rbLapInfo.LapPlateStart)
                            {
                                rbLapInfo.LapCouplerStart = false;
                                rbLapInfo.LapWeldStart = false;
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_START, rbLapInfo.LapCouplerStart ? "1" : "0");
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_START, rbLapInfo.LapWeldStart ? "1" : "0");
                            }

                            if (rbLapInfo.LapWeldEnd)
                            {
                                rbLapInfo.LapCouplerEnd = false;
                                rbLapInfo.LapPlateEnd = false;
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_END, rbLapInfo.LapCouplerEnd ? "1" : "0");
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_END, rbLapInfo.LapPlateEnd ? "1" : "0");
                            }
                            if (rbLapInfo.LapCouplerEnd)
                            {
                                rbLapInfo.LapWeldEnd = false;
                                rbLapInfo.LapPlateEnd = false;
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_END, rbLapInfo.LapWeldEnd ? "1" : "0");
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_END, rbLapInfo.LapPlateEnd ? "1" : "0");
                            }
                            if (rbLapInfo.LapPlateEnd)
                            {
                                rbLapInfo.LapCouplerEnd = false;
                                rbLapInfo.LapWeldEnd = false;
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_END, rbLapInfo.LapCouplerEnd ? "1" : "0");
                                rb.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_END, rbLapInfo.LapWeldEnd ? "1" : "0");
                            }
                        }
                        else
                        {
                            var rebarLapSchema = JsonConvert.DeserializeObject<RevRebarLap>(rebarLapSchemaInfo.Value);
                            if (rebarLapSchema.LapWeldStart != rbLapInfo.LapWeldStart)
                            {
                                if (rbLapInfo.LapWeldStart)
                                {
                                    rbLapInfo.LapCouplerStart = false;
                                    rbLapInfo.LapPlateStart = false;
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_START, rbLapInfo.LapCouplerStart ? "1" : "0");
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_START, rbLapInfo.LapPlateStart ? "1" : "0");
                                }
                            }
                            if (rebarLapSchema.LapWeldEnd != rbLapInfo.LapWeldEnd)
                            {
                                if (rbLapInfo.LapWeldEnd)
                                {
                                    rbLapInfo.LapCouplerEnd = false;
                                    rbLapInfo.LapPlateEnd = false;
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_END, rbLapInfo.LapCouplerEnd ? "1" : "0");
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_END, rbLapInfo.LapPlateEnd ? "1" : "0");
                                }
                            }
                            if (rebarLapSchema.LapCouplerStart != rbLapInfo.LapCouplerStart)
                            {
                                if (rbLapInfo.LapCouplerStart)
                                {
                                    rbLapInfo.LapWeldStart = false;
                                    rbLapInfo.LapPlateStart = false;
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_START, rbLapInfo.LapWeldStart ? "1" : "0");
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_START, rbLapInfo.LapPlateStart ? "1" : "0");
                                }
                            }
                            if (rebarLapSchema.LapCouplerEnd != rbLapInfo.LapCouplerEnd)
                            {
                                if (rbLapInfo.LapCouplerEnd)
                                {
                                    rbLapInfo.LapWeldEnd = false;
                                    rbLapInfo.LapPlateEnd = false;
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_END, rbLapInfo.LapWeldEnd ? "1" : "0");
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_PLATE_END, rbLapInfo.LapPlateEnd ? "1" : "0");
                                }
                            }
                            if (rebarLapSchema.LapPlateStart != rbLapInfo.LapPlateStart)
                            {
                                if (rbLapInfo.LapPlateStart)
                                {
                                    rbLapInfo.LapCouplerStart = false;
                                    rbLapInfo.LapWeldStart = false;
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_START, rbLapInfo.LapCouplerStart ? "1" : "0");
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_START, rbLapInfo.LapWeldStart ? "1" : "0");
                                }
                            }
                            if (rebarLapSchema.LapPlateEnd != rbLapInfo.LapPlateEnd)
                            {
                                if (rbLapInfo.LapPlateEnd)
                                {
                                    rbLapInfo.LapCouplerEnd = false;
                                    rbLapInfo.LapWeldEnd = false;
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_COUPLER_END, rbLapInfo.LapCouplerEnd ? "1" : "0");
                                    rb.SetParameterValue(Properties.ParameterRebarLap.LAP_WELD_END, rbLapInfo.LapWeldEnd ? "1" : "0");
                                }
                            }
                        }
                        try
                        {
                            var bb = rb.get_BoundingBox(doc.ActiveView);
                            var els = bb.GetElementAroundBox(doc, BuiltInCategory.OST_GenericModel)
                                .Where(x => SchemaInfo.ReadAll(m_schemaInfoLap.SchemaBase, m_schemaInfoLap.SchemaField, x) != null)
                                .Where(x =>
                                {
                                    var info = JsonConvert.DeserializeObject<RevLap>(SchemaInfo.ReadAll(m_schemaInfoLap.SchemaBase, m_schemaInfoLap.SchemaField, x).Value);
                                    return info.Hostid.ToString() == rb.Id.ToString();
                                }).ToList();
                            foreach (var el in els)
                            {
                                try
                                {
                                    if (el.Pinned)
                                        el.Pinned = false;
                                    doc.Regenerate();
                                    doc.Delete(el.Id);
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        RevLap.CreateLap(rb, rbLapInfo);
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

        public string GetAdditionalInformation() => "Rebar Updater";
        public ChangePriority GetChangePriority() => ChangePriority.Rebar;
        public UpdaterId GetUpdaterId() => m_updaterId;
        public string GetUpdaterName() => "Rebar Updater";
        public static void Init(UIControlledApplication application)
        {
            var rebarUpdater = new RebarModifyUpdater(application.ActiveAddInId);
            UpdaterRegistry.RegisterUpdater(rebarUpdater);
            ElementClassFilter rebarFilter = new ElementClassFilter(typeof(Rebar));
            UpdaterRegistry.AddTrigger(rebarUpdater.GetUpdaterId(), rebarFilter,
                                   Element.GetChangeTypeAny());
        }
        public static void Dispose(UIControlledApplication application)
        {
            RebarModifyUpdater updater = new RebarModifyUpdater(application.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
        }
    }
}
