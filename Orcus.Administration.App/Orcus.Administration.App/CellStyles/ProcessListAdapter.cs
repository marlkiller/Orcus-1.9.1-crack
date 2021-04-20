using System;
using Android.Widget;
using System.Collections.Generic;
using Android.App;

namespace Orcus.Administration.App
{
	public class ProcessListAdapter : ArrayAdapter <AdvancedProcessInfo>
	{
		Activity context;

		public ProcessListAdapter (Activity context, List<AdvancedProcessInfo> processes)
			: base(context, Android.Resource.Id.Text1, processes)
		{
			this.context = context;
			NotifyDataSetChanged ();
			Processes = processes;
		}

		public void Change(List<AdvancedProcessInfo> processes){

			this.Clear ();
			foreach (var process in processes) {
				Add (process);
			}

			Processes = processes;
		}

		public List<AdvancedProcessInfo> Processes {
			get;
			private set;
		}

		public override Android.Views.View GetView (int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
		{
			var view = context.LayoutInflater.Inflate(Resource.Layout.ProcessRow, null);

			var item = GetItem(position);

			var imageView = view.FindViewById<ImageView> (Resource.Id.icon);
			if (item.Icon != null)
				imageView.SetImageBitmap (item.Icon);
			else
				imageView.SetImageResource (Resource.Drawable.application);

			var text1 = view.FindViewById<TextView> (Resource.Id.nameText);
			text1.Text = item.Name;
			text1.TextSize = 24;

			return view;
		}
	}
}

