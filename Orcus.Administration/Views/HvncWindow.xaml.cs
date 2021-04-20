using System.Collections.Generic;
using System.Windows.Input;
using Orcus.Administration.ViewModels.CommandViewModels;
using Orcus.Shared.Commands.HVNC;

namespace Orcus.Administration.Views
{
    /// <summary>
    ///     Interaction logic for HvncWindow.xaml
    /// </summary>
    public partial class HvncWindow
    {
#if DEBUG
        private readonly HvncViewModel _hvncViewModel;
        private readonly List<Key> _keysPressed;

        public HvncWindow(HvncViewModel hvncViewModel)
        {
            _hvncViewModel = hvncViewModel;
            InitializeComponent();
            DataContext = hvncViewModel;
            _keysPressed = new List<Key>();
        }
#endif
        private void HvncScreenImage_OnKeyDown(object sender, KeyEventArgs e)
        {
#if DEBUG
            if (HvncScreenImage.Source != null && EnableKeyboardCheckBox.IsChecked == true &&
                _hvncViewModel.IsRunning)
            {
                e.Handled = true;

                _hvncViewModel.HvncCommand.KeyboardAction((byte) KeyInterop.VirtualKeyFromKey(e.Key),
                    true);
            }
#endif
        }

        private void HvncScreenImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
#if DEBUG
            if (HvncScreenImage.Source != null && EnableMouseCheckBox.IsChecked == true &&
                _hvncViewModel.IsRunning)
            {
                var p = e.GetPosition(HvncScreenImage);
                var remoteX = GetRemoteWidth(p.X);
                var remoteY = GetRemoteHeight(p.Y);

                _hvncViewModel.HvncCommand.MouseAction(
                    e.ChangedButton == MouseButton.Left ? HvncAction.LeftDown : HvncAction.RightDown,
                    remoteX, remoteY);
            }
#endif
        }

        private void HvncScreenImage_OnMouseMove(object sender, MouseEventArgs e)
        {
#if DEBUG
            return;
            if (HvncScreenImage.Source != null && EnableMouseCheckBox.IsChecked == true &&
                _hvncViewModel.IsRunning)
            {
                var p = e.GetPosition(HvncScreenImage);
                var remoteX = GetRemoteWidth(p.X);
                var remoteY = GetRemoteHeight(p.Y);

                _hvncViewModel.HvncCommand.MouseAction(HvncAction.MouseMove, remoteX, remoteY);
            }
#endif
        }

        private void HvncScreenImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
#if DEBUG
            if (HvncScreenImage.Source != null && EnableMouseCheckBox.IsChecked == true &&
                _hvncViewModel.IsRunning)
            {
                var p = e.GetPosition(HvncScreenImage);
                var remoteX = GetRemoteWidth(p.X);
                var remoteY = GetRemoteHeight(p.Y);

                _hvncViewModel.HvncCommand.MouseAction(
                    e.ChangedButton == MouseButton.Left ? HvncAction.LeftUp : HvncAction.RightUp,
                    remoteX, remoteY);
            }
#endif
        }

#if DEBUG
        private int GetRemoteWidth(double localX)
        {
            return (int) (localX/HvncScreenImage.ActualWidth*_hvncViewModel.RenderEngine.ScreenWidth);
        }

        private int GetRemoteHeight(double localY)
        {
            return (int) (localY/HvncScreenImage.ActualHeight*_hvncViewModel.RenderEngine.ScreenHeight);
        }
#endif
        private void OpenProcessTextBox_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenProcessTextBox.Focusable = true;
            OpenProcessTextBox.Focus();
        }
    }
}