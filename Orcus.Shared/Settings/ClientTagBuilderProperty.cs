using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class ClientTagBuilderProperty : IBuilderProperty
    {
        public string ClientTag { get; set; } = ""; //IMPORTANT

        public IBuilderProperty Clone()
        {
            return new ClientTagBuilderProperty {ClientTag = ClientTag};
        }
    }
}