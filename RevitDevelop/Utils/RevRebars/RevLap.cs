using Autodesk.Revit.DB.Structure;
using Newtonsoft.Json;
using RevitDevelop.Utils.Entities;
using RevitDevelop.Utils.GraphicInViews;
using RevitDevelop.Utils.Solids;

namespace RevitDevelop.Utils.RevRebars
{
    public class RevLap
    {
        public static Autodesk.Revit.DB.Color Revit_Color_Weld = new Autodesk.Revit.DB.Color(0, 0, 255);
        public static Autodesk.Revit.DB.Color Revit_Color_Coupler = new Autodesk.Revit.DB.Color(0, 255, 0);
        public static Autodesk.Revit.DB.Color Revit_Color_Lap_InValid = new Autodesk.Revit.DB.Color(127, 127, 127);
        public int Hostid { get; set; }
        public int LapType { get; set; }
        public RevLap() { }
        public RevLap(int hostid, int lapType)
        {
            Hostid = hostid;
            LapType = lapType;
        }
        public static void CreateLap(Rebar rebar, RevRebarLap revRebarLap)
        {
            try
            {
                var m_schemaInfoRebar = new SchemaInfo(
                Properties.PropertySchemalInfo.SCHEMAL_REBAR_LAP_GUID,
                Properties.PropertySchemalInfo.SCHEMAL_REBAR_LAP_NAME,
                new SchemaField());
                var schemainfo = new SchemaInfo(
                    Properties.PropertySchemalInfo.SCHEMAL_LAP_GUID,
                    Properties.PropertySchemalInfo.SCHEMAL_LAP_NAME,
                    new SchemaField());
                var document = rebar.Document;
                var width = 50;
                var cs = rebar.GetLinesOrigin();
                var sp = cs.FirstOrDefault().GetEndPoint(0);
                var ep = cs.LastOrDefault().GetEndPoint(1);
                var revLapInfoStart = new RevLap();
                var revLapInfoEnd = new RevLap();
                revLapInfoStart.Hostid = int.Parse(rebar.Id.ToString());
                revLapInfoEnd.Hostid = int.Parse(rebar.Id.ToString());
                DirectShape lapStart = null;
                DirectShape lapEnd = null;
                Color colorStart = null;
                Color colorEnd = null;

                if (revRebarLap.LapWeldStart)
                {
                    revLapInfoStart.LapType = (int)RevRebarLapType.Weld;
                    colorStart = Revit_Color_Weld;
                }
                if (revRebarLap.LapCouplerStart)
                {
                    revLapInfoStart.LapType = (int)RevRebarLapType.Coupler;
                    colorStart = Revit_Color_Coupler;
                }
                if (revRebarLap.LapPlateStart)
                {
                    revLapInfoStart.LapType = (int)RevRebarLapType.PlateStop;
                    colorStart = Revit_Color_Coupler;
                }
                if (revRebarLap.LapWeldEnd)
                {
                    revLapInfoEnd.LapType = (int)RevRebarLapType.Weld;
                    colorEnd = Revit_Color_Weld;
                }
                if (revRebarLap.LapCouplerEnd)
                {
                    revLapInfoEnd.LapType = (int)RevRebarLapType.Coupler;
                    colorEnd = Revit_Color_Coupler;
                }
                if (revRebarLap.LapPlateEnd)
                {
                    revLapInfoEnd.LapType = (int)RevRebarLapType.PlateStop;
                    colorEnd = Revit_Color_Coupler;
                }

                if (revRebarLap.LapWeldStart || revRebarLap.LapCouplerStart || revRebarLap.LapPlateStart)
                {
                    lapStart = sp.CreateSolid(XYZ.BasisZ, width, width).CreateDirectShape(document);
                    document.ActiveView.SetTransparenColorElement(lapStart, colorStart, 0);
                    lapStart.Pinned = true;
                }
                if (revRebarLap.LapWeldEnd || revRebarLap.LapCouplerEnd || revRebarLap.LapPlateEnd)
                {
                    lapEnd = ep.CreateSolid(XYZ.BasisZ, width, width).CreateDirectShape(document);
                    document.ActiveView.SetTransparenColorElement(lapEnd, colorEnd, 0);
                    lapEnd.Pinned = true;
                }
                if (lapStart != null)
                {
                    schemainfo.SchemaField.Value = JsonConvert.SerializeObject(revLapInfoStart);
                    SchemaInfo.Write(schemainfo.SchemaBase, lapStart, schemainfo.SchemaField);
                    revRebarLap.HostStartID = int.Parse(lapStart.Id.ToString());
                }
                if (lapEnd != null)
                {
                    schemainfo.SchemaField.Value = JsonConvert.SerializeObject(revLapInfoEnd);
                    SchemaInfo.Write(schemainfo.SchemaBase, lapEnd, schemainfo.SchemaField);
                    revRebarLap.HostEndID = int.Parse(lapEnd.Id.ToString());
                }

                m_schemaInfoRebar.SchemaField.Value = JsonConvert.SerializeObject(revRebarLap);
                SchemaInfo.Write(m_schemaInfoRebar.SchemaBase, rebar, m_schemaInfoRebar.SchemaField);
            }
            catch (Exception)
            {
            }
        }
    }
}
