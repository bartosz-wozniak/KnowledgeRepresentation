using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace KnowledgeRepresentation.App.Common
{
    /// <summary>
    ///     Custom dialog manager class
    /// </summary>
    internal sealed class CustomDialogManager : ICustomDialogManager
    {
        /// <summary>
        ///     Displays message box on the screen
        /// </summary>
        /// <param name="title">title</param>
        /// <param name="message">message</param>
        public async Task DisplayMessageBox(string title, string message)
        {
            var metroWindow = Application.Current.MainWindow as MetroWindow;
            // ReSharper disable once PossibleNullReferenceException
            metroWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            await metroWindow.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, metroWindow.MetroDialogOptions);
        }
    }
}
