using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Orcus.Administration.ViewModels.CommandViewModels;

namespace Orcus.Administration.Views.CommandViews
{
    /// <summary>
    ///     Interaction logic for AudioVolumeControlCommandView.xaml
    /// </summary>
    public partial class AudioVolumeControlCommandView
    {
        public AudioVolumeControlCommandView()
        {
            InitializeComponent();
        }

        private void PlaybackThumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var slider = (Slider) sender;
            ((AudioVolumeControlViewModel) DataContext).SetSelectedPlaybackDeviceChannelVolume((int) slider.Tag,
                (float) slider.Value);
        }

        private void RecordingThumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var slider = (Slider) sender;
            ((AudioVolumeControlViewModel) DataContext).SetSelectedRecordingDeviceChannelVolume((int) slider.Tag,
                (float) slider.Value);
        }
    }
}