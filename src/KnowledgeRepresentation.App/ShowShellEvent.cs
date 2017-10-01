using System;

namespace KnowledgeRepresentation.App
{
    /// <summary>
    ///     Event invocator to show shell view.
    /// </summary>
    public sealed class ShowShellEvent
    {
        /// <summary>
        ///     Constructor of the class
        /// </summary>
        /// <param name="viewModel">type of shell view model</param>
        public ShowShellEvent(Type viewModel)
        {
            ViewModel = viewModel;
        }

        /// <summary>
        ///     Type of shell view model
        /// </summary>
        public Type ViewModel { get; }
    }
}
