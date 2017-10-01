using System.Threading.Tasks;

namespace KnowledgeRepresentation.App.Common
{
    /// <summary>
    ///     Custom dialog manager interface
    /// </summary>
    internal interface ICustomDialogManager
    {
        /// <summary>
        ///     Displays message box on the screen
        /// </summary>
        /// <param name="title">title</param>
        /// <param name="message">message</param>
        Task DisplayMessageBox(string title, string message);
    }
}
