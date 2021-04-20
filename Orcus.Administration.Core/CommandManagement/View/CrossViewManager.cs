using System;
using System.Collections.Generic;
using Orcus.Administration.Plugins.CommandViewPlugin;

namespace Orcus.Administration.Core.CommandManagement.View
{
    public class CrossViewManager : ICrossViewManager
    {
        private readonly Dictionary<Guid, ICrossViewMethod> _methods;

        public CrossViewManager()
        {
            _methods = new Dictionary<Guid, ICrossViewMethod>();
        }

        public bool ContainsMethod(Guid methodGuid)
        {
            return _methods.ContainsKey(methodGuid);
        }

        public void RegisterMethod<T>(ICommandView commandView, Guid methodGuid, EventHandler<T> eventHandler)
        {
            if (_methods.ContainsKey(methodGuid))
                throw new ArgumentException("There is already a method with the give guid");

            _methods.Add(methodGuid, new CrossViewMethod<T>(commandView, eventHandler));
        }

        public void ExecuteMethod(Guid methodGuid, object parameter)
        {
            ICrossViewMethod crossViewMethod;
            if (!_methods.TryGetValue(methodGuid, out crossViewMethod))
                throw new ArgumentException("No method found with the given guid");

            crossViewMethod.Execute(parameter);
            OpenCommandView?.Invoke(this, crossViewMethod.CommandView);
        }

        public event EventHandler<ICommandView> OpenCommandView;
    }

    public class CrossViewMethod<T> : ICrossViewMethod
    {
        private readonly EventHandler<T> _eventHandler;

        public CrossViewMethod(ICommandView commandView, EventHandler<T> eventHandler)
        {
            CommandView = commandView;
            _eventHandler = eventHandler;
        }

        public ICommandView CommandView { get; }

        void ICrossViewMethod.Execute(object parameter)
        {
            Execute((T) parameter);
        }

        public void Execute(T parameter)
        {
            _eventHandler?.Invoke(this, parameter);
        }
    }

    public interface ICrossViewMethod
    {
        ICommandView CommandView { get; }
        void Execute(object parameter);
    }
}