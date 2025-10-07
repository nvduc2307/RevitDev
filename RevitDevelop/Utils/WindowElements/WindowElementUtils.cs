using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RevitDevelop.Utils.WindowElements
{
    public static class WindowElementUtils
    {
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject root) where T : DependencyObject
        {
            if (root == null) yield break;

            var stack = new Stack<DependencyObject>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    if (child is T t) yield return t;
                    stack.Push(child);
                }
            }
        }
    }
}
