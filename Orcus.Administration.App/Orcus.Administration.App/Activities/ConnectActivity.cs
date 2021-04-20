
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;

namespace Orcus.Administration.App
{
	[Activity (Label = "Orcus Administration", MainLauncher = true)]			
	public class ConnectActivity : Activity
	{
		Button ConnectButton;
		CheckBox RememberPasswordCheckBox;
		EditText IpTextBox;
		EditText PortTextBox;
		EditText PasswordTextBox;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			SetContentView (Resource.Layout.ConnectLayout);

			// Create your application here
			ConnectButton = FindViewById<Button> (Resource.Id.ConnectButton);
			RememberPasswordCheckBox = FindViewById<CheckBox> (Resource.Id.RememberPasswordCheckBox);
			IpTextBox = FindViewById<EditText> (Resource.Id.IpAddressTextBox);
			PortTextBox = FindViewById<EditText> (Resource.Id.PortTextBox);
			PasswordTextBox = FindViewById<EditText> (Resource.Id.PasswordTextBox);

			IpTextBox.Text = Settings.IpAddress;
			PortTextBox.Text = Settings.Port.ToString();
			PasswordTextBox.Text = Settings.Password;
			RememberPasswordCheckBox.Checked = Settings.RememberPassword;

			ConnectButton.Click += ConnectButton_Click;
		}

		async void ConnectButton_Click (object sender, EventArgs e)
		{
			var progressDialog = ProgressDialog.Show (this, "Connecting", "Trying to connect...", true);
			ConnectionResult result;
			result = await	Task.Run (() => ConnectionManager.ConnectToServer (IpTextBox.Text.Trim(), int.Parse(PortTextBox.Text), PasswordTextBox.Text, (s) => RunOnUiThread(new Action(() => progressDialog.SetMessage (s)))));
			progressDialog.Dismiss ();
			progressDialog.Dispose ();

			switch (result) {
			case ConnectionResult.Awesome:
				Settings.IpAddress = IpTextBox.Text.Trim();
				Settings.Port = int.Parse (PortTextBox.Text);
				Settings.RememberPassword = RememberPasswordCheckBox.Checked;

				if (RememberPasswordCheckBox.Checked)
					Settings.Password = PasswordTextBox.Text;
				else
					Settings.Password = string.Empty;

				StartActivity (typeof(ClientsActivity));
				break;
			case ConnectionResult.AuthenticateException:
				Toast.MakeText (this, "Couldn't authenticate. Invalid password.", ToastLength.Long).Show();
				break;
			case ConnectionResult.ServerNotResponsing:
				Toast.MakeText (this, "Server not responsing.", ToastLength.Long).Show();
				break;
			case ConnectionResult.InvalidVersion:
				Toast.MakeText (this, "Invalid api version.", ToastLength.Long).Show();
				break;
			default:
				break;
			}
		}
	}
}