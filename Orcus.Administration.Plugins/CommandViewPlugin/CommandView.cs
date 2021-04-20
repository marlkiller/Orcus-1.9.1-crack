using System.Windows.Media;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.Plugins.CommandViewPlugin
{
    /// <summary>
    ///     A base class for a <see cref="ICommandView" />
    /// </summary>
    public abstract class CommandView : PropertyChangedBase, ICommandView
    {
        private ImageSource _icon;

        private IWindowService _windowService;

        /// <summary>
        ///     The client controller for this command view
        /// </summary>
        protected IClientController ClientController;

        /// <summary>
        ///     The cross view manager for this command view
        /// </summary>
        protected ICrossViewManager CrossViewManager;

        /// <summary>
        ///     Override to release resources
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        ///     The name of the menu item
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     The category of the menu item
        /// </summary>
        public abstract Category Category { get; }

        /// <summary>
        ///     Provides methods to open windows modal to the window the command currently lives in
        /// </summary>
        public IWindowService WindowService
        {
            get { return _windowService; }
            set { SetProperty(value, ref _windowService); }
        }

        /// <summary>
        ///     An icon which represents this command
        /// </summary>
        public ImageSource Icon => _icon ?? (_icon = GetIconImageSource());

        /// <summary>
        ///     Constructor of the class. Here, you can store the commands you need in variables (you get them from the
        ///     <see cref="IClientController" />)
        /// </summary>
        /// <param name="clientController">Provides information about the client you can use</param>
        /// <param name="crossViewManager">Provides methods to communicate between different views</param>
        public void Initialize(IClientController clientController, ICrossViewManager crossViewManager)
        {
            ClientController = clientController;
            CrossViewManager = crossViewManager;
            InitializeView(clientController, crossViewManager);
        }

        /// <summary>
        ///     Called when the view is visible for the first time
        /// </summary>
        /// <param name="loadData">
        ///     Determines whether the data should be loaded automatically or the user wants to do that
        ///     manually
        /// </param>
        public virtual void LoadView(bool loadData)
        {
        }

        /// <summary>
        ///     Can be overridden to give the command an icon
        /// </summary>
        /// <returns>The icon which represents this command</returns>
        protected virtual ImageSource GetIconImageSource()
        {
            return null;
        }

        /// <summary>
        ///     Initialize the command view
        /// </summary>
        /// <param name="clientController">Provides information about the client you can use</param>
        /// <param name="crossViewManager">Provides methods to communicate between different views</param>
        protected abstract void InitializeView(IClientController clientController, ICrossViewManager crossViewManager);
    }
}