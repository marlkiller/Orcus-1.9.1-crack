using System;
using System.Text;

namespace Orcus.Administration.App
{
	public class CommandToken
	{
		protected internal CommandToken()
		{

		}

		public byte[] TokenBytes { get; set; }

		public static CommandToken CreateToken(string token)
		{
			if (string.IsNullOrEmpty(token))
				throw new ArgumentException("Token can not be empty");

			var bytes = Encoding.ASCII.GetBytes(token);
			if (bytes.Length != 2) throw new ArgumentException("Token must have a length of two chars");

			return new CommandToken { TokenBytes = bytes };
		}
	}
}