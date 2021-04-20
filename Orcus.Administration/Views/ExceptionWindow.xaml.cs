using System;
using System.Windows;
using Orcus.Administration.ViewModels;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow
    {
        public ExceptionWindow(Exception exception)
        {
            InitializeComponent();
            var dataContext = new ExceptionDialogViewModel(exception);
            dataContext.Close += DataContext_Close;
            DataContext = dataContext;
        }

        private void DataContext_Close(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}