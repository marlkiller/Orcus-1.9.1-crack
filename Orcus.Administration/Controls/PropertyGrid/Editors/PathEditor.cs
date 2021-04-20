using System.Windows;
using System.Windows.Controls;
using Orcus.Administration.Controls.PropertyGrid.Controls;
using Orcus.Plugins.PropertyGrid.Attributes;

namespace Orcus.Administration.Controls.PropertyGrid.Editors
{
    public class PathEditor : PropertyEditor<PathBox>
    {
        private readonly PathMode _pathMode;
        private readonly string _filter;

        public PathEditor(PathMode pathMode, string filter)
        {
            _pathMode = pathMode;
            _filter = filter;
        }

        protected override DependencyProperty GetDependencyProperty()
        {
            return TextBox.TextProperty;
        }

        protected override void InitializeControl()
        {
            base.InitializeControl();
            Editor.IsSelectingFile = _pathMode == PathMode.File;
            Editor.Filter = _filter;
            Editor.BorderThickness = new Thickness(0);
            Editor.MinHeight = 20;
        }
    }
}