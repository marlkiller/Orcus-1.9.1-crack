
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;

namespace Orcus.Administration.App
{
	public class ConsoleFragment : Fragment
	{
		private TextView _consoleOutputTextView;
		private ScrollView _scrollView;
		private EditText _inputEditText;
		private Button _sendButton;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override void OnDetach ()
		{
			base.OnDetach ();
			var consoleCommand = ConnectionManager.Current.CurrentController?.Commander.GetCommand<ConsoleCommand> ();
			if (consoleCommand != null) {
				consoleCommand.Started -= ConsoleCommand_Started;
				consoleCommand.Stopped -= ConsoleCommand_Stopped;
				consoleCommand.ConsoleLineReceived -= ConsoleCommand_ConsoleLineReceived;
			}
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			var view = inflater.Inflate (Resource.Layout.ConsoleFragment, null);

			var consoleCommand = ConnectionManager.Current.CurrentController.Commander.GetCommand<ConsoleCommand> ();
			consoleCommand.Started += ConsoleCommand_Started;
			consoleCommand.Stopped += ConsoleCommand_Stopped;
			consoleCommand.ConsoleLineReceived += ConsoleCommand_ConsoleLineReceived;

			_consoleOutputTextView = view.FindViewById<TextView> (Resource.Id.consoleOutputTextView);
			_scrollView = view.FindViewById<ScrollView> (Resource.Id.mainScrollView);
			_inputEditText = view.FindViewById<EditText> (Resource.Id.InputEditText);
			_sendButton = view.FindViewById<Button> (Resource.Id.SendButton);
			_sendButton.Click += (object sender, EventArgs e) => { consoleCommand.SendCommand(_inputEditText.Text); _inputEditText.Text = null;};;
			_inputEditText.TextChanged += _inputEditText_TextChanged;

			_consoleOutputTextView.Text = consoleCommand.CurrentOutput?.ToString () + "\r\n";
			if (consoleCommand.IsEnabled) {
				_inputEditText.Enabled = true;
			}

			return view;
		}

		void _inputEditText_TextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			_sendButton.Enabled = !string.IsNullOrWhiteSpace (_inputEditText.Text);
		}

		void ConsoleCommand_ConsoleLineReceived (object sender, string e)
		{
			if(Activity != null)
				Activity.RunOnUiThread (async () => {_consoleOutputTextView.Text += e + "\r\n";
				await Task.Delay(50);
					if(_inputEditText.Bottom > 0)
						_scrollView.SmoothScrollBy(0, _inputEditText.Bottom);
			});
		}

		void ConsoleCommand_Stopped (object sender, EventArgs e)
		{
			if (Activity != null)
				Activity.RunOnUiThread (() => {
					_sendButton.Enabled = false;
					_inputEditText.Enabled = false;
					_inputEditText.Text = null;
				});
		}

		void ConsoleCommand_Started (object sender, EventArgs e)
		{
			if (Activity != null)
				Activity.RunOnUiThread (() => {
					_consoleOutputTextView.Text = null;
					_inputEditText.Enabled = true;
				});
		}
	}
}

