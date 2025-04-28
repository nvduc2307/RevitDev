using Autodesk.Revit.DB;
using RevitDevelop.Utils.FilterElementsInRevit;
using RevitDevelop.Utils.Geometries;
using RevitDevelop.Utils.NumberUtils;
using RevitDevelop.Utils.RevFaces;
using RevitDevelop.Utils.RevTitleBlocks;
using System.Windows.Media;

namespace RevitDevelop.Utils.RevViewScales
{
    public class RevViewScale
    {
        public const string SCALE_1_200 = "1/200";
        public const string SCALE_1_250 = "1/250";
        public const string SCALE_1_300 = "1/300";
        public const string SCALE_1_400 = "1/400";
        public const string SCALE_1_500 = "1/500";
        public const string SCALE_1_600 = "1/600";
        public const string SCALE_1_800 = "1/800";
        public const int DISTANCE_70 = 70;
        public const int DISTANCE_80 = 80;
        public const int DISTANCE_100 = 100;
        public const int DISTANCE_110 = 110;
        public const int DISTANCE_120 = 120;
        public const int DISTANCE_160 = 160;
        public const int DISTANCE_180 = 180;
        public const int DISTANCE_230 = 230;
        public const int DISTANCE_240 = 240;
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public int FrameDrawing { get; set; }
        public static List<RevViewScale> GetRevViewScale()
        {
            var scs = new List<RevViewScale>();
            scs.Add(new RevViewScale() { Id = 0, Name = SCALE_1_200, Value = 200, FrameDrawing = 0 });
            scs.Add(new RevViewScale() { Id = 1, Name = SCALE_1_250, Value = 250, FrameDrawing = 0 });
            scs.Add(new RevViewScale() { Id = 2, Name = SCALE_1_300, Value = 300, FrameDrawing = 0 });
            scs.Add(new RevViewScale() { Id = 3, Name = SCALE_1_400, Value = 400, FrameDrawing = 0 });
            scs.Add(new RevViewScale() { Id = 4, Name = SCALE_1_500, Value = 500, FrameDrawing = 0 });
            scs.Add(new RevViewScale() { Id = 5, Name = SCALE_1_600, Value = 600, FrameDrawing = 0 });
            scs.Add(new RevViewScale() { Id = 6, Name = SCALE_1_800, Value = 800, FrameDrawing = 0 });
            //scs.Add(new RevViewScale() { Id = 7, Name = SCALE_1_200, Value = 200, FrameDrawing = 2 });
            //scs.Add(new RevViewScale() { Id = 8, Name = SCALE_1_250, Value = 250, FrameDrawing = 2 });
            //scs.Add(new RevViewScale() { Id = 9, Name = SCALE_1_300, Value = 300, FrameDrawing = 2 });
            //scs.Add(new RevViewScale() { Id = 10, Name = SCALE_1_400, Value = 400, FrameDrawing = 2 });
            //scs.Add(new RevViewScale() { Id = 11, Name = SCALE_1_500, Value = 500, FrameDrawing = 2 });
            //scs.Add(new RevViewScale() { Id = 12, Name = SCALE_1_600, Value = 600, FrameDrawing = 2 });
            //scs.Add(new RevViewScale() { Id = 13, Name = SCALE_1_800, Value = 800, FrameDrawing = 2 });
            return scs;
        }

        /// <summary>
        /// Tính toán tỷ lệ thích hợp cho bản vẽ dựa vào kích thước lưới và khổ giấy
        /// </summary>
        /// <param name="doc">Revit Document hiện tại</param>
        /// <param name="paperSize">Khổ giấy ("A1" hoặc "A3")</param>
        /// <returns>Tỷ lệ ở dạng chuỗi "1:XXX"</returns>
        /// 
        public static RevViewScale CalculateDrawingScale(
            Document doc, 
            RevTitleBlockType paperSizeType, 
            List<RevViewScale> revViewScales)
        {
            var allGrids = doc.GetElementsFromClass<Grid>(false);
            List<Grid> xGrids = new List<Grid>();
            List<Grid> yGrids = new List<Grid>();

            foreach (Grid grid in allGrids.Cast<Grid>())
            {
                Line gridLine = grid.Curve as Line;
                if (gridLine == null) continue;
                XYZ direction = gridLine.Direction.Normalize();
                if (Math.Abs(direction.X) > Math.Abs(direction.Y))
                    yGrids.Add(grid);
                else
                    xGrids.Add(grid);
            }
            // Tính toán khoảng cách X (giữa các grid theo hướng North-South)
            double xDistance = CalculateMaxGridDistance(xGrids, XYZ.BasisY) * 0.3048;
            // Tính toán khoảng cách Y (giữa các grid theo hướng East-West)
            double yDistance = CalculateMaxGridDistance(yGrids, XYZ.BasisX) * 0.3048;
            var result = revViewScales.FirstOrDefault(); // Mặc định

            switch (paperSizeType)
            {
                case RevTitleBlockType.A1:
                    if (xDistance <= DISTANCE_120 && yDistance <= DISTANCE_70)
                        result = revViewScales.FirstOrDefault(x => x.Name == SCALE_1_200);
                    else if (xDistance <= DISTANCE_160 && yDistance <= DISTANCE_80)
                        result = revViewScales.FirstOrDefault(x => x.Name == SCALE_1_250);
                    else if (xDistance <= DISTANCE_180 && yDistance <= DISTANCE_100)
                        result = revViewScales.FirstOrDefault(x => x.Name == SCALE_1_300);
                    else if (xDistance <= DISTANCE_230 && yDistance <= DISTANCE_100)
                        result = revViewScales.FirstOrDefault(x => x.Name == SCALE_1_400);
                    else
                        result = revViewScales.FirstOrDefault(x => x.Name == SCALE_1_300); // Trường hợp đặc biệt cho bản vẽ 2 trang
                    break;
                case RevTitleBlockType.A3:
                    if (xDistance <= DISTANCE_120 && yDistance <= DISTANCE_70)
                        result = revViewScales.FirstOrDefault(x => x.Name == SCALE_1_400);
                    else if (xDistance <= DISTANCE_160 && yDistance <= DISTANCE_80)
                        result = revViewScales.FirstOrDefault(x => x.Name == SCALE_1_500);
                    else if (xDistance <= DISTANCE_180 && yDistance <= DISTANCE_100)
                        result = revViewScales.FirstOrDefault(x => x.Name == SCALE_1_600);
                    else if (xDistance <= DISTANCE_230 && yDistance <= DISTANCE_100)
                        result = revViewScales.FirstOrDefault(x => x.Name == SCALE_1_800);
                    else
                        result = revViewScales.FirstOrDefault(x => x.Name == SCALE_1_600); // Trường hợp đặc biệt cho bản vẽ 2 trang
                    break;
            }
            return result;
        }
        /// <summary>
        /// Tính khoảng cách tối đa giữa các grid
        /// </summary>
        /// <param name="grids">Danh sách các grid</param>
        /// <returns>Khoảng cách tối đa (trong đơn vị nội bộ Revit)</returns>
        private static double CalculateMaxGridDistance(List<Grid> grids, XYZ vt)
        {
            double maxDistance = 0;
            // Sắp xếp grid theo vị trí
            var sortedGrids = SortGridsByPosition(grids);
            Grid firstGrid = sortedGrids.First();
            Grid lastGrid = sortedGrids.Last();
            Line firstLine = firstGrid.Curve as Line;
            Line lastLine = lastGrid.Curve as Line;
            var pStart = firstLine.GetEndPoint(0);
            var face = new FaceCustom(vt, pStart);
            var pEnd = lastLine.GetEndPoint(0).RayPointToFace(vt, face);
            maxDistance = pStart.DistanceTo(pEnd);
            return maxDistance;
        }
        /// <summary>
        /// Sắp xếp các grid theo vị trí không gian
        /// </summary>
        /// <param name="grids">Danh sách grid</param>
        /// <returns>Danh sách grid đã sắp xếp</returns>
        private static List<Grid> SortGridsByPosition(List<Grid> grids)
        {
            if (grids.Count <= 1) return grids;
            // Lấy hướng tham chiếu từ grid đầu tiên
            Line firstLine = grids[0].Curve as Line;
            if (firstLine == null) return grids;
            XYZ direction = firstLine.Direction.Normalize();
            XYZ perpDirection = new XYZ(-direction.Y, direction.X, 0).Normalize();
            // Sắp xếp grid theo vị trí dọc theo hướng vuông góc với grid
            return grids.OrderBy(g =>
            {
                Line line = g.Curve as Line;
                if (line == null) return 0;
                // Lấy điểm giữa của đường grid
                XYZ midPoint = (line.GetEndPoint(0) + line.GetEndPoint(1)) * 0.5;
                // Tính toán giá trị phép chiếu để sắp xếp
                return midPoint.DotProduct(perpDirection);
            }).ToList();
        }
    }
}
