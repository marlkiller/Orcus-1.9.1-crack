using System;
using System.ComponentModel;
using System.Windows;
using Orcus.Administration.Plugins.CommandViewPlugin;

namespace Orcus.Administration.ViewModels.ViewInterface
{
    public interface IWindow : IWindowService
    {
        event EventHandler Closed;
        event CancelEventHandler Closing;

        void Close();

        WindowState WindowState { get; set; }
    }
}