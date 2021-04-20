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

namespace Orcus.Administration.App
{
	public class TaskManagerFragment : ListFragment
	{
		private TaskManagerCommand taskManagerCommand;

		public override void OnDetach ()
		{
			base.OnDetach ();
			taskManagerCommand.RefreshList -= TaskManagerCommand_RefreshList;
			taskManagerCommand.TaskKilled -= TaskManagerCommand_TaskKilled;
			taskManagerCommand.TaskKillFailed -= TaskManagerCommand_TaskKillFailed;
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			taskManagerCommand = ConnectionManager.Current.CurrentController.Commander.GetCommand<TaskManagerCommand> ();
			taskManagerCommand.RefreshList += TaskManagerCommand_RefreshList;
			taskManagerCommand.TaskKilled += TaskManagerCommand_TaskKilled;
			taskManagerCommand.TaskKillFailed += TaskManagerCommand_TaskKillFailed;

			ListAdapter = new ProcessListAdapter (Activity, taskManagerCommand.Processes ?? new List<AdvancedProcessInfo> ());
		}

		void TaskManagerCommand_TaskKillFailed (object sender, EventArgs e)
		{
			Activity?.RunOnUiThread (() => Toast.MakeText (Activity, "Task kill failed", ToastLength.Long).Show ());
		}

		void TaskManagerCommand_TaskKilled (object sender, EventArgs e)
		{
			Activity?.RunOnUiThread (() => Toast.MakeText (Activity, "Task killed successfully", ToastLength.Long).Show ());
			taskManagerCommand.Refresh ();
		}

		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);
			RegisterForContextMenu (ListView);
		}

		public override void OnCreateContextMenu (IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
		{
			base.OnCreateContextMenu (menu, v, menuInfo);

			var info = (Android.Widget.AdapterView.AdapterContextMenuInfo) menuInfo;
			var process = ((ProcessListAdapter)ListAdapter).Processes [info.Position];

			menu.Add("Kill");
			menu.SetHeaderTitle (process.Name);
		}

		public override bool OnContextItemSelected (IMenuItem item)
		{
			var info = (Android.Widget.AdapterView.AdapterContextMenuInfo) item.MenuInfo;
			var process = ((ProcessListAdapter)ListAdapter).Processes [info.Position];
			taskManagerCommand.KillProcess (process);

			return base.OnContextItemSelected (item);
		}

		public override void OnListItemClick (ListView l, View v, int position, long id)
		{
			base.OnListItemClick (l, v, position, id);
			var item = ((ProcessListAdapter)ListAdapter).Processes [position];
			Toast.MakeText (Activity, item.Name, ToastLength.Short);
		}

		void TaskManagerCommand_RefreshList (object sender, List<AdvancedProcessInfo> e)
		{
			Activity?.RunOnUiThread (() => ((ProcessListAdapter)ListAdapter).Change (e));
		}
	}
}