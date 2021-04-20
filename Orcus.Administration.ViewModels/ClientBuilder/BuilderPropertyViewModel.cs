using Orcus.Plugins.Builder;
using Orcus.Shared.Core;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.ClientBuilder
{
    public class BuilderPropertyViewModel : PropertyChangedBase
    {
        private bool _failed;
        private string _failMessage;
        private bool _isEnabled;

        public BuilderPropertyViewModel(IBuilderProperty builderProperty)
        {
            BuilderProperty = builderProperty;
            _isEnabled = true;
        }

        public bool IsFromPlugin { get; set; }
        public IBuilderProperty BuilderProperty { get; }
        public IBuilderPropertyView BuilderPropertyView { get; set; }

        public bool Failed
        {
            get { return _failed; }
            set { SetProperty(value, ref _failed); }
        }

        public string FailMessage
        {
            get { return _failMessage; }
            set { SetProperty(value, ref _failMessage); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(value, ref _isEnabled); }
        }
    }
}