using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.DaitoTest.Utils
{
    public static class ModelCloudUltis
    {
        // Thư mục cache mặc định Revit 2024
        static string CacheRoot =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "Autodesk", "Revit", "Autodesk Revit 2024", "CollaborationCache");

        /// <summary>
        /// Tìm (projectGuid, modelGuid) trong CollaborationCache rồi mở cloud model tương ứng.
        /// </summary>
        public static List<string> OpenFromCacheGuids(this UIApplication uiapp, Func<string, bool> modelNameFilter = null)
        {
            var result = new List<string>();
            // Duyệt tất cả thư mục con, cố gắng bắt cặp projectGuid / modelGuid từ tên thư mục
            foreach (var dirProj in Directory.EnumerateDirectories(CacheRoot, "*", SearchOption.TopDirectoryOnly))
            {
                if (!Guid.TryParse(Path.GetFileName(dirProj), out Guid projectGuid)) continue;

                foreach (var dirModel in Directory.EnumerateDirectories(dirProj, "*", SearchOption.TopDirectoryOnly))
                {
                    var modelFolder = Path.GetFileName(dirModel);
                    if (!Guid.TryParse(modelFolder, out Guid modelGuid)) continue;

                    // (Tuỳ) nếu bạn muốn lọc theo tên hiển thị (đọc từ một file metadata của bạn)
                    if (modelNameFilter != null && !modelNameFilter(modelFolder)) continue;

                    result.Add(modelFolder);
                }
            }

            return result;
        }
    }
}
