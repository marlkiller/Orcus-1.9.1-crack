using System;
using Orcus.Shared.Core;

namespace Orcus.Shared.Settings
{
    [Serializable]
    public class ProxyBuilderProperty : IBuilderProperty
    {
        public ProxyOption ProxyOption { get; set; } = ProxyOption.None;
        public string ProxyAddress { get; set; }
        public int ProxyPort { get; set; } = 1080;
        public int ProxyType { get; set; } = 2;

        public IBuilderProperty Clone()
        {
            return new ProxyBuilderProperty
            {
                ProxyOption = ProxyOption,
                ProxyAddress = ProxyAddress,
                ProxyPort = ProxyPort
            };
        }
    }

    public enum ProxyOption
    {
        None,
        AutomaticDetection,
        CustomProxy
    }
}