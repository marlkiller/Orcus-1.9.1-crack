using System;
using Plugin.Settings.Abstractions;
using Plugin.Settings;

namespace Orcus.Administration.App
{
	public static class Settings
	{
		private const string IpAddressKey = "ipaddress_key";
		private static readonly string IpAddressDefault = string.Empty;

		private const string PortKey = "port_key";
		private static readonly int PortDefault = 10134;

		private const string PasswordKey = "password_key";
		private static readonly string PasswordDefault = string.Empty;

		private const string RememberPasswordKey = "rememberpassword_key";
		private static readonly bool RememberPasswordDefault = true;

		private static ISettings AppSettings
		{
			get
			{
				return CrossSettings.Current;
			}
		}

		public static string IpAddress
		{
			get { return AppSettings.GetValueOrDefault<string>(IpAddressKey, IpAddressDefault); }
			set { AppSettings.AddOrUpdateValue<string>(IpAddressKey, value); }
		}

		public static int Port
		{
			get { return AppSettings.GetValueOrDefault<int>(PortKey, PortDefault); }
			set { AppSettings.AddOrUpdateValue<int>(PortKey, value); }
		}

		public static string Password
		{
			get { return AppSettings.GetValueOrDefault<string>(PasswordKey, PasswordDefault); }
			set { AppSettings.AddOrUpdateValue<string>(PasswordKey, value); }
		}

		public static bool RememberPassword
		{
			get { return AppSettings.GetValueOrDefault<bool>(RememberPasswordKey, RememberPasswordDefault); }
			set { AppSettings.AddOrUpdateValue<bool>(RememberPasswordKey, value); }
		}
	}
}