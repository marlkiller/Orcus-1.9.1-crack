using System;

namespace Orcus.Administration.App
{
	public abstract class Command
	{
		private uint? _identifier;
		protected ConnectionInfo ConnectionInfo;

		protected event EventHandler Loaded;

		/// <summary>
		/// Execute the function. This method is only executed on the computer of the victim
		/// </summary>
		/// <param name="parameter">The parameter without token</param>
		public abstract void ResponseReceived(byte[] parameter);

		/// <summary>
		/// Initializes the command
		/// </summary>
		/// <param name="connectionInfo">Some information about the connection</param>
		public void Initialize(ConnectionInfo connectionInfo)
		{
			ConnectionInfo = connectionInfo;
			if (Loaded != null)
				Loaded.Invoke (this, EventArgs.Empty);
		}

		/// <summary>
		/// The command ID
		/// </summary>
		public uint Identifier{ get { return (_identifier ?? (_identifier = GetId ())).Value; } }

		protected abstract uint GetId();
	}
}