using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using Orcus.Shared.Connection;
using Android.Graphics.Drawables;
using Android.Graphics;

namespace Orcus.Administration.App
{
	public class ClientListItem_Adapter : ArrayAdapter <LightClientInformationApp> {
		Activity context;
		readonly List<LightClientInformationApp> _clients;

		public ClientListItem_Adapter(Activity context, List<LightClientInformationApp> clients)
			: base(context, Android.Resource.Id.Text1, GenerateView(clients))
		{
			this.context = context;
			this.NotifyDataSetChanged ();
			_clients = clients;
		}

		public void Change(){

			this.Clear ();
			foreach (var client in GenerateView(_clients)) {
				this.Add (client);
			}
		}

		public LightClientInformationApp GetClientFromPosition(int position){
			return GetItem (position);
		}

		private static List<LightClientInformationApp> GenerateView(List<LightClientInformationApp> clients){
			var result = new List<LightClientInformationApp> ();
			foreach (var group in clients.GroupBy(x => x.Group)) {
				result.Add (new SectionClientInfo (group.Key));
				result.AddRange (group);
			}
			return result;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = GetItem(position);

			if (item != null) {
				var section = item as SectionClientInfo;
				if (section != null) {
					var view2 = context.LayoutInflater.Inflate(Resource.Layout.ClientListSectionItem, null);
					view2.SetOnClickListener (null);
					view2.SetOnLongClickListener (null);
					view2.LongClickable = false;

					var textView = view2.FindViewById<TextView> (Resource.Id.sectionTextView);
					textView.Text = string.IsNullOrEmpty (item.Group) ? "Default" : item.Group;
					return view2;
				}

				var view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem2, null);
				var text1 = view.FindViewById<TextView> (Android.Resource.Id.Text1);
				text1.Text = item.UserName;
				text1.TextSize = 24;
				text1.SetTextColor (Color.Black);

				var text2 = view.FindViewById<TextView> (Android.Resource.Id.Text2);
				text2.Text = item.IpAddress;
				text2.SetTextColor (Color.Black);

				return view;
			}

			return null;
		}
	}
}