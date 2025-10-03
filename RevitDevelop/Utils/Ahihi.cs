using revUI = Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RevitDevelop.Utils
{
    public static class ScreenModelMapper
    {
        [DllImport("user32.dll")] static extern bool GetCursorPos(out System.Drawing.Point lpPoint);

        // 1) Lấy điểm model từ vị trí chuột hiện tại (orthographic views)
        public static bool TryGetMouseModelPoint(revUI.UIDocument uidoc, System.Drawing.Point pointCusor, out XYZ pModelOnViewPlane)
        {
            pModelOnViewPlane = null;

            revUI.UIView uiv = uidoc.GetOpenUIViews().FirstOrDefault(v => v.ViewId == uidoc.ActiveView.Id);
            if (uiv == null) return false;

            if (pointCusor == null) return false;
            return TryScreenToModel(uidoc.ActiveView, uiv, pointCusor, out pModelOnViewPlane);
        }

        // 2) Chuyển pixel → model (trên mặt phẳng view)
        public static bool TryScreenToModel(View view, revUI.UIView uiv, System.Drawing.Point screenPt, out XYZ p)
        {
            p = null;

            // Pixel rect của view
            var rect = uiv.GetWindowRectangle();
            var r = new System.Drawing.Rectangle()
            {
                X = rect.Left,
                Y = rect.Top,
                Width = Math.Abs(rect.Right - rect.Left),
                Height = Math.Abs(rect.Top - rect.Bottom),
            };
            if (screenPt.X < r.Left || screenPt.X > r.Right || screenPt.Y < r.Top || screenPt.Y > r.Bottom) return false;

            // Chuẩn hóa u,v trong [0,1]
            double u = (screenPt.X - r.Left) / (double)(r.Right - r.Left);
            double v = (screenPt.Y - r.Top) / (double)(r.Bottom - r.Top); // screen Y đi xuống

            // Hai góc zoom trong tọa độ model (trên mặt phẳng view)
            var ps = uiv.GetZoomCorners();
            var pMin = new XYZ(ps.Min(x => x.X), ps.Min(x => x.Y), ps.Min(x => x.Z));
            var pMax = new XYZ(ps.Max(x => x.X), ps.Max(x => x.Y), ps.Max(x => x.Z));

            XYZ R = view.RightDirection; // phương ngang của view
            XYZ U = view.UpDirection;    // phương dọc của view
            XYZ diag = pMax - pMin;

            double w = diag.DotProduct(R); // chiều rộng theo R
            double h = diag.DotProduct(U); // chiều cao theo U

            // Nội suy: vModel = 1 - vScreen
            p = pMin + (u * w) * R + ((1 - v) * h) * U;
            return true;
        }

        // 3) Chuyển model (điểm bất kỳ) → pixel
        //    Nếu p không nằm trên mặt phẳng view, sẽ chiếu vuông góc xuống mặt phẳng view trước.
        public static bool TryModelToScreen(View view, revUI.UIView uiv, XYZ modelPt, out System.Drawing.Point screenPt)
        {
            screenPt = System.Drawing.Point.Empty;

            // Góc pixel & góc model
            var rect = uiv.GetWindowRectangle();
            var r = new System.Drawing.Rectangle()
            {
                X = rect.Left,
                Y = rect.Top,
                Width = Math.Abs(rect.Right - rect.Left),
                Height = Math.Abs(rect.Top - rect.Bottom),
            };
            var ps = uiv.GetZoomCorners();
            var pMin = new XYZ(ps.Min(x=>x.X), ps.Min(x => x.Y), ps.Min(x => x.Z));
            var pMax = new XYZ(ps.Max(x=>x.X), ps.Max(x => x.Y), ps.Max(x => x.Z));

            // Cơ sở tọa độ của view
            XYZ R = view.RightDirection;
            XYZ U = view.UpDirection;
            XYZ N = view.ViewDirection;      // pháp tuyến mặt phẳng view
            XYZ O = view.Origin;

            // Chiếu modelPt xuống mặt phẳng view
            XYZ q = ProjectToPlane(modelPt, O, N);

            XYZ diag = pMax - pMin;
            double w = diag.DotProduct(R);
            double h = diag.DotProduct(U);

            // Tọa độ cục bộ theo cơ sở (R,U)
            XYZ local = q - pMin;
            double u = local.DotProduct(R) / w;   // 0..1
            double v = 1 - (local.DotProduct(U) / h);

            int x = r.Left + (int)Math.Round(u * (r.Right - r.Left));
            int y = r.Top + (int)Math.Round(v * (r.Bottom - r.Top));

            screenPt = new System.Drawing.Point(x, y);
            return true;
        }

        private static XYZ ProjectToPlane(XYZ q, XYZ origin, XYZ normal)
        {
            return q - normal * ((q - origin).DotProduct(normal));
        }
    }
}
