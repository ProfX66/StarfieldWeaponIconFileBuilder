using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CustomMessageBox.Avalonia;
using StarfieldWeaponIconFileBuilder.Extensions;
using System;
using System.Threading.Tasks;

namespace StarfieldWeaponIconFileBuilder.Utilities;

public static class Message
{
    #region Properties

    private static string HelpSuffix => "\n\nYou can review logs in:\n%LogPath%\n\nFeel free to contact me at:\nhttp:\\\\pxcnet.xyz";

    #endregion

    #region Methods

    /// <summary>
    /// Shows a avalonia message box with the specified message, title, and icon.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="icon"></param>
    /// <param name="buttons"></param>
    /// <param name="defaultButtons"></param>
    /// <returns></returns>
    public static async Task<MessageBoxResult> Show(string message, string title = "Message", MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxDefaultButton defaultButtons = MessageBoxDefaultButton.Button1)
    {
        await Logging.AdvisoryAsync($"Showing message [ {title} ] to user:\n{message}\n");

        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            MessageBox messageBox = new(message, title, icon)
            {
                HorizontalButtonsPanelAlignment = HorizontalAlignment.Center,
                Background = AppBrushes.ConstellationDarkBlueBrush
            };

            messageBox.Opened += (sender, e) =>
            {
                if (messageBox.FindControl<StackPanel>("PART_ButtonsPanel") is StackPanel buttonsPanel)
                {
                    foreach (var child in buttonsPanel.Children)
                    {
                        if (child is Button btn && btn.Classes.Contains("accent"))
                        {
                            btn.FontWeight = FontWeight.Bold;
                        }
                    }
                }
            };

            return await messageBox.Show(buttons, defaultButtons, "accent");
        });
    }

    /// <summary>
    /// Shows a avalonia message box tailored to errors and exception data
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="exception"></param>
    /// <param name="appendSuffix"></param>
    /// <returns></returns>
    public static async Task ShowError(string message, string title = "Error!", Exception? exception = null, bool appendSuffix = true)
    {
        string helpSuffix = HelpSuffix.ExpandVariables()!;
        string finalMessage = message;
        if (!exception.IsNullOrEmpty())
        {
            finalMessage += $"\n\nException: {exception.Message}";
            await Logging.ExceptionAsync(exception);
        }
        if (appendSuffix) finalMessage += helpSuffix;

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Show(finalMessage, title, MessageBoxIcon.Error);
        });
    }

    #endregion
}
