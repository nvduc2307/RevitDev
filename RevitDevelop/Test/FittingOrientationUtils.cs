namespace RevitDevelop.Test
{
    public static class FittingOrientationUtils
    {
        private const double EPS = 1e-9;

        public static void AlignByHandFacing(
            FamilyInstance fi,
            XYZ targetHand,
            XYZ targetFacing,
            XYZ newOrigin = null)   // null = giữ nguyên vị trí
        {
            if (fi == null) throw new ArgumentNullException(nameof(fi));
            if (targetHand == null || targetFacing == null)
                throw new ArgumentNullException("Target vectors must not be null.");

            var doc = fi.Document;

            // ===== hệ trục hiện tại =====
            var curHand = fi.HandOrientation.Normalize();   // X
            var curFacing = fi.FacingOrientation.Normalize(); // Z
            var curUp = curHand.CrossProduct(curFacing).Normalize(); // Y

            // ===== hệ trục target =====
            var tgtHand = targetHand.Normalize();   // X
            var tgtFacing = targetFacing.Normalize(); // Z
            var tgtUp = tgtHand.CrossProduct(tgtFacing).Normalize(); // Y

            // ===== 1) Move nếu cần =====
            var curOrigin = fi.GetTransform().Origin;
            var finalOrigin = newOrigin ?? curOrigin;

            // move tới origin mới (nếu khác)
            var move = finalOrigin - curOrigin;
            if (move.GetLength() > EPS)
                ElementTransformUtils.MoveElement(doc, fi.Id, move);

            // ===== 2) Tính rotation từ hệ cur -> tgt =====
            // matrix 3x3, cột là basis X,Y,Z
            double[,] from =
            {
                    { curHand.X, curUp.X, curFacing.X },
                    { curHand.Y, curUp.Y, curFacing.Y },
                    { curHand.Z, curUp.Z, curFacing.Z }
                };

            double[,] to =
            {
                    { tgtHand.X, tgtUp.X, tgtFacing.X },
                    { tgtHand.Y, tgtUp.Y, tgtFacing.Y },
                    { tgtHand.Z, tgtUp.Z, tgtFacing.Z }
                };

            // R = to * from^T
            var R = Multiply3x3(to, Transpose3x3(from));

            // axis-angle từ ma trận quay
            if (!GetAxisAngle(R, out XYZ axis, out double angle))
            {
                // gần như không cần xoay

                return;
            }

            // tạo line axis đi qua origin mới
            var axisLine = Line.CreateUnbound(finalOrigin, axis);

            ElementTransformUtils.RotateElement(doc, fi.Id, axisLine, angle);
        }

        // ===== matrix helper =====

        private static double[,] Transpose3x3(double[,] m)
        {
            return new double[3, 3]
            {
            { m[0,0], m[1,0], m[2,0] },
            { m[0,1], m[1,1], m[2,1] },
            { m[0,2], m[1,2], m[2,2] }
            };
        }

        private static double[,] Multiply3x3(double[,] A, double[,] B)
        {
            var r = new double[3, 3];
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                    r[i, j] = A[i, 0] * B[0, j] + A[i, 1] * B[1, j] + A[i, 2] * B[2, j];
            return r;
        }

        /// <summary>
        /// Lấy axis (unit) + angle (rad) từ ma trận quay 3x3.
        /// </summary>
        private static bool GetAxisAngle(double[,] R, out XYZ axis, out double angle)
        {
            axis = null;
            angle = 0;

            double trace = R[0, 0] + R[1, 1] + R[2, 2];
            double cos = (trace - 1.0) / 2.0;
            cos = Math.Max(-1.0, Math.Min(1.0, cos));
            angle = Math.Acos(cos);

            if (angle < 1e-6) // không cần xoay
                return false;

            double sin = Math.Sin(angle);
            if (Math.Abs(sin) < 1e-6)
            {
                // gần 180°, case khó; tạm coi như không hỗ trợ ở đây
                return false;
            }

            double x = (R[2, 1] - R[1, 2]) / (2.0 * sin);
            double y = (R[0, 2] - R[2, 0]) / (2.0 * sin);
            double z = (R[1, 0] - R[0, 1]) / (2.0 * sin);

            axis = new XYZ(x, y, z).Normalize();
            return axis.GetLength() > EPS;
        }
    }
}