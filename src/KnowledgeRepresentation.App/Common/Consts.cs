namespace KnowledgeRepresentation.App.Common
{
    /// <summary>
    ///     Constants
    /// </summary>
    internal static class Consts
    {
        internal static class Language
        {
            internal static class Sentences
            {
                internal const string Causes = "A lasts d causes a if pi";

                internal const string Releases = "A lasts d releases f if pi";

                internal const string Triggers = "pi triggers A";

                internal const string Invokes = "A invokes B after d if pi";

                internal const string Impossible = "A impossible at t";
            }

            internal static class Scenario
            {
                internal const string Text = "ScenarioName:\n    T = d\n    ACS = (A, d)\n    OBS = (f, d) (f, d)";
            }

            internal static class Queries
            {
                internal const string AlwaysExecutable = "always executable S";

                internal const string EverExecutable = "ever executable S";

                internal const string AlwaysHolds = "always holds pi at t when S";

                internal const string EverHolds = "ever holds pi at t when S";

                internal const string AlwaysOccurs = "A always occurs at t when S";

                internal const string EverOccurs = "A ever occurs at t when S";
            }
        }

        internal static class Labels
        {
            internal const string InputHello = "Load a file or write Your text here. ";

            internal const string OutputText = "Nothing has been computed yet. ";
        }

        internal static class FileFilters
        {
            internal const string Txt = "txt files (*.txt)|*.txt";
        }

        internal static class Errors
        {
            internal const string Title = "Error";

            internal const string Info = "An error occured. Operation was terminated. ";
        }

        internal static class Window
        {
            internal const string Title = "Knowledge Representation";
        }

        internal static class Mappings
        {
            internal const string ViewModel = "KnowledgeRepresentation.App.ViewModels";

            internal const string View = "KnowledgeRepresentation.App.Views";
        }

        internal static class Endings
        {
            internal const string ViewModel = "ViewModel";

            internal const string View = "View";
        }
    }
}
