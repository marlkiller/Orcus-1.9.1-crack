using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Orcus.Shared.Connection;

namespace Orcus.Administration.App
{
	public class ClientListFragment : ListFragment
	{
		string groupName;
		List<LightClientInformationApp> clients;

		public static ClientListFragment NewInstance(int position, string groupName)
		{
			var f = new ClientListFragment ();
			var b = new Bundle ();
			b.PutInt("position", position);
			b.PutString ("group", groupName);
			f.Arguments = b;

			return f;
		}

		public override void OnDetach ()
		{
			base.OnDetach ();
			ConnectionManager.Current.ClientDisconnected -= ConnectionManager_Current_ClientDisconnected;
			ConnectionManager.Current.ClientConnected -= ConnectionManager_Current_ClientConnected;
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
			groupName = Arguments.GetString("group");
			clients = ConnectionManager.Current.Clients.Where(x => x.Group == groupName).ToList();
			ListAdapter = new ClientListItem_Adapter (Activity, clients);

			ConnectionManager.Current.ClientDisconnected += ConnectionManager_Current_ClientDisconnected;
			ConnectionManager.Current.ClientConnected += ConnectionManager_Current_ClientConnected;
		}

		void ConnectionManager_Current_ClientConnected (object sender, OnlineClientInformation e)
		{
			if (e.Group == groupName && Activity != null) {
				clients = ConnectionManager.Current.Clients.Where(x => x.Group == groupName).ToList();
				Activity.RunOnUiThread (() => ((ClientListItem_Adapter)ListAdapter).Change ());
			}
		}

		void ConnectionManager_Current_ClientDisconnected (object sender, BaseClientInformation e)
		{
			if (e.Group == groupName && Activity != null) {
				clients = ConnectionManager.Current.Clients.Where(x => x.Group == groupName).ToList();
				Activity.RunOnUiThread (() => ((ClientListItem_Adapter)ListAdapter).Change ());
			}
		}

		public override void OnListItemClick (Android.Widget.ListView lValue, Android.Views.View vValue, int position, long id)
		{
			var client = clients[position];
			ConnectionManager.Current.LogIn (client.Id);
		}
	}
}
