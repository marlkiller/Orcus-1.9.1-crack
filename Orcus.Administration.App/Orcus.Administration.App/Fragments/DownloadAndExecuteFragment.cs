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
	public class DownloadAndExecuteFragment : Fragment
	{
		private EditText _urlEditText;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Create your fragment here
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			// Use this to return your custom view for this Fragment
			// return inflater.Inflate(Resource.Layout.YourFragment, container, false);
			var view = inflater.Inflate (Resource.Layout.DownloadAndExecuteFragment, null);

			_urlEditText = view.FindViewById<EditText> (Resource.Id.UrlEditText);
			if (savedInstanceState != null) {
				_urlEditText.Text = savedInstanceState.GetString ("urlText");
			}

			view.FindViewById<Button>(Resource.Id.DownloadButton).Click += (sender, e) => {
				var url = view.FindViewById<EditText>(Resource.Id.UrlEditText).Text;
				if(string.IsNullOrWhiteSpace(url))
				{
					Toast.MakeText(Activity, "Url is empty", ToastLength.Short).Show();
					return;
				}

				new AlertDialog.Builder(Activity)
					.SetPositiveButton("Yes", (s, args) =>
						{
							ConnectionManager.Current.CurrentController.Commander.GetCommand<InternetCommand>().DownloadAndExecuteFile(url);
							Toast.MakeText(Activity, "The file will be downloaded and executed", ToastLength.Long).Show();
						})
					.SetNegativeButton("Cancel", (s2, args) =>
						{
							// User pressed no
						})
					.SetMessage(!System.Uri.IsWellFormedUriString(url, UriKind.Absolute) ? "The url \"" + url + "\" doesn't seem like an uri, are you sure that you want to continue?" : "Download and excute file from url \"" +url+ "\"?")
					.SetTitle("Warning")
					.Show();
			};

			return view;
		}

		public override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
			outState.PutString ("urlText", _urlEditText.Text);
		}
	}
}

