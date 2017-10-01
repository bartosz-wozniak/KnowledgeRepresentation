using Caliburn.Micro;
using KnowledgeRepresentation.App.Common;

namespace KnowledgeRepresentation.App.ViewModels
{
    /// <summary>
    ///     Shell view model class
    /// </summary>
    internal sealed class ShellViewModel : Conductor<object>.Collection.OneActive, IHandle<ShowShellEvent>
    {
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        ///     Constructor of the class
        /// </summary>
        /// <param name="eventAggregator">Event Aggregator</param>
        public ShellViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        ///     ShowShellEvent handler
        /// </summary>
        public void Handle(ShowShellEvent @event)
        {
            var viewModel = @event.ViewModel;
            ActivateItem(IoC.GetInstance(viewModel, string.Empty));
        }

        protected override void OnInitialize()
        {
            _eventAggregator.Subscribe(this);
            DisplayName = Consts.Window.Title;
            ActivateItem(IoC.Get<MainWindowViewModel>());
        }
    }
}
