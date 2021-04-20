using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orcus.Administration.Core.CommandManagement;
using Orcus.CommandManagement;

namespace Orcus.Test
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod, TestCategory("Commands")]
        public void UniqueCommandIdsTest()
        {
            using (var commandSelector = new CommandSelector())
            {
                var commandTokens = new List<uint>();
                foreach (var command in commandSelector.CommandCollection)
                {
                    if (commandTokens.Any(x => x == command.Identifier))
                        Assert.Fail($"Command tokens are doubled. Command: {command}");

                    commandTokens.Add(command.Identifier);
                }
            }
        }

        [TestMethod, TestCategory("Commands")]
        public void UniqueStaticCommandIdsTest()
        {
            var commandTokens = new List<Guid>();
            var staticCommands = StaticCommander.GetStaticCommands();
            foreach (var staticCommand in staticCommands)
            {
                if (commandTokens.Any(x => x == staticCommand.CommandId))
                    Assert.Fail($"Command tokens are doubled. Command: {staticCommand} and {staticCommands.First(x => x.CommandId == staticCommand.CommandId)}");

                commandTokens.Add(staticCommand.CommandId);
            }
        }
    }
}