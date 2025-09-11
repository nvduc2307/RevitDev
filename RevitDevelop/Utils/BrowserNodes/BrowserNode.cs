using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace RevitDevelop.Utils.BrowserNodes
{
    public class BrowserNode : ObservableObject
    {
        private bool _isSelected;
        public string Name { get; set; }
        public long ViewId { get; set; }
        public long ViewSheetId { get; set; }
        public BrowserNode Parent { get; set; }
        public List<BrowserNode> Children { get; set; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                SelectAction?.Invoke(this, new EventArgs());
                ExternalAction?.Invoke();
            }
        }
        public EventHandler SelectAction { get; set; }
        public Action ExternalAction { get; set; }
        public BrowserNode(string name)
        {
            Name = name;
            ViewId = -1;
            ViewSheetId = -1;
            Children = new List<BrowserNode>();
        }
    }
    public enum NodeKind
    {
        View,
        Sheet,
        Schedule,
    }
}
