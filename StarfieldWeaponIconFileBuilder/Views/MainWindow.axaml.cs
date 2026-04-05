using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CustomMessageBox.Avalonia;
using StarfieldWeaponIconFileBuilder.Extensions;
using StarfieldWeaponIconFileBuilder.Models;
using StarfieldWeaponIconFileBuilder.Utilities;
using StarfieldWeaponIconFileBuilder.ViewModels;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Message = StarfieldWeaponIconFileBuilder.Utilities.Message;

namespace StarfieldWeaponIconFileBuilder.Views
{
    public partial class MainWindow : Window
    {
        #region Properties

        private readonly DispatcherTimer _resizeTimer;
        public static AppConfig? AppConfig { get; set; }
        public static LoggingData? LogConfig { get; set; }
        private static bool PassedPrereqs { get; set; } = true;
        private MainWindowViewModel? VM => DataContext as MainWindowViewModel;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            LogConfig = new LoggingData { UseLocalAppData = true };
            AppConfig = new AppConfig(Environment.GetCommandLineArgs(), Assembly.GetExecutingAssembly(), LogConfig);

            InitializeComponent();

            _resizeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };

            _resizeTimer.Tick += (_, __) =>
            {
                _resizeTimer.Stop();
                UpdatePreview();
            };

            SizeChanged += (_, __) =>
            {
                _resizeTimer.Stop();
                _resizeTimer.Start();
            };
        }

        #endregion

        #region Window Events

        /// <summary>
        /// Window and elements fully rendered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Opened(object? sender, EventArgs e)
        {
            VM?.AppendWindowTitle($"v{Logging.AppVersionString}");
            InitializeApp();
        }

        /// <summary>
        /// Dispose logging on window closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object? sender, WindowClosingEventArgs e)
        {
            AppConfig.DisposeAndExit();
        }
        
        /// <summary>
        /// Update the SVG preview render when the window changes size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdatePreview();
        }

        #endregion

        #region Click Events

        /// <summary>
        /// Display a folder browser to the user based on the sender tag element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnBrowseFolderClick(object? sender, RoutedEventArgs e)
        {
            string PathProperty = "None";
            if (sender is Button btn && btn.Tag is string action)
            {
                if (!action.IsNullOrEmptyOrWhiteSpace())
                    PathProperty = action;
            }

            FolderPickerOpenOptions options = new()
            {
                Title = "Select export folder",
                AllowMultiple = false
            };

            IReadOnlyList<IStorageFolder>? result = null;
            try
            {
                result = await this.StorageProvider.OpenFolderPickerAsync(options);
            }
            catch (Exception Ex)
            {
                await Message.ShowError("Failed to show folder browser dialog.", exception: Ex);
            }

            if (!result.IsNullOrEmpty() && result.Count > 0 && !VM.IsNullOrEmpty())
            {
                var file = result[0];
                switch (PathProperty)
                {
                    case "ExportPath":
                        VM.ExportPath = file.Path.LocalPath;
                        break;
                    case "CloneExportPath":
                        VM.CloneExportPath = file.Path.LocalPath;
                        break;
                    default:
                        VM.ExportPath = file.Path.LocalPath;
                        break;
                }

            }
        }

        /// <summary>
        /// Display a file browser to the user based on the sender tag element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnBrowseFileClick(object? sender, RoutedEventArgs e)
        {
            string extension = "*.*";
            string PathProperty = "None";

            if (sender is Button btn && btn.Tag is string action)
            {
                if (!action.IsNullOrEmptyOrWhiteSpace())
                {
                    switch (action)
                    {
                        case "SvgPath":
                            PathProperty = "SvgPath";
                            extension = "*.SVG";
                            break;
                        case "CloneSourcePath":
                            PathProperty = "CloneSourcePath";
                            extension = "*.SWF";
                            break;
                        case "WeaponIconTemplatePath":
                            PathProperty = "WeaponIconTemplatePath";
                            extension = "*.SWF";
                            break;
                        case "JavaPath":
                            PathProperty = "JavaPath";
                            extension = "Java.exe";
                            break;
                        case "FfdecPath":
                            PathProperty = "FfdecPath";
                            extension = "ffdec-cli.jar";
                            break;
                        default:
                            PathProperty = "SvgPath";
                            extension = "*.SVG";
                            break;
                    }
                }
            }

            string title = "Select a file";
            string caption = $"{extension.ToUpper()} Files";
            if (!extension.EndsWith('*')) { title = $"Select a {extension.ToUpper()} file"; }
            if (!extension.StartsWith('*'))
            {
                title = $"Find and select {extension}";
                caption = $"{extension} File";
            }

            FilePickerOpenOptions options = new()
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType(caption)
                    {
                        Patterns = [extension.ToLower()]
                    }
                ]
            };

            IReadOnlyList<IStorageFile>? result = null;
            try
            {
                result = await this.StorageProvider.OpenFilePickerAsync(options);
            }
            catch (Exception Ex)
            {
                await Message.ShowError("Failed to show file browser dialog.", exception: Ex);
            }

            if (!result.IsNullOrEmpty() && result.Count > 0 && !VM.IsNullOrEmpty())
            {
                IStorageFile file = result[0];
                switch (PathProperty)
                {
                    case "SvgPath":
                        VM.SvgPath = file.Path.LocalPath;
                        VM.LoadSvgPreview(400, 400);
                        break;
                    case "CloneSourcePath":
                        VM.CloneSourcePath = file.Path.LocalPath;
                        break;
                    case "JavaPath":
                        VM.AppSettings!.JavaPath = file.Path.LocalPath;
                        break;
                    case "WeaponIconTemplatePath":
                        VM.AppSettings!.WeaponIconTemplatePath = ConvertToAppPath(file.Path.LocalPath);
                        break;
                    case "FfdecPath":
                        VM.AppSettings!.FfdecPath = ConvertToAppPath(file.Path.LocalPath);
                        break;
                    default:
                        VM.SvgPath = file.Path.LocalPath;
                        VM.LoadSvgPreview(400, 400);
                        break;
                }
            }

            VM?.NotifyValidationChanged();
        }

        /// <summary>
        /// Creates the new Weapon Icon file from the template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CreateButton_Click(object? sender, RoutedEventArgs e)
        {
            ToggleLoadingOverlay(true, "FinalFilePath");
            bool? result = null;
            string linkageName = VM!.WeaponLinkageName!;
            string exportPath = VM.ExportPath!;
            string svgPath = VM.SvgPath!;
            bool autosize = VM.AppSettings!.AutoResizeIcon;

            await Task.Run(() =>
            {
                result = Starfield.NewWeaponIconFile(linkageName, exportPath, svgPath, autosize);
                System.Threading.Thread.Sleep(2000);
            });

            if (!result.GetValueOrDefault())
            {
                await Message.ShowError($"There was an error attempting to create:\n{VM?.FinalFilePath}");
            }
            else
            {
                MessageBoxResult mmsgResult = await Message.Show($"Successfully created icon file:\n{VM?.FinalFilePath}\n\nWould you like to reset the form to make another?", "Success!", MessageBoxIcon.Information, MessageBoxButtons.YesNo);
                if (mmsgResult == MessageBoxResult.Yes)
                {
                    VM!.ResetCreate();
                }
            }

            ToggleLoadingOverlay(false);
        }

        /// <summary>
        /// Creates a copy of the passed Weapon Icon file with the provided name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CloneCreateButton_Click(object? sender, RoutedEventArgs e)
        {
            ToggleLoadingOverlay(true, "CloneFinalFilePath");
            bool? result = null;
            string linkageName = VM!.CloneWeaponLinkageName!;
            string sourcePath = VM.CloneSourcePath!;
            string exportPath = VM.CloneExportPath!;

            await Task.Run(() =>
            {
                result = Starfield.CloneWeaponIconFile(linkageName, sourcePath, exportPath);
                System.Threading.Thread.Sleep(2000);
            });

            if (!result.GetValueOrDefault())
            {
                await Message.ShowError($"There was an error attempting to copy [ {sourcePath.GetFileName()} ] to:\n{VM?.CloneFinalFilePath}");
            }
            else
            {
                MessageBoxResult mmsgResult = await Message.Show($"Successfully cloned icon file [ {sourcePath.GetFileName()} ] to:\n{VM?.CloneFinalFilePath}\n\nWould you like to reset the form to make another?", "Success!", MessageBoxIcon.Information, MessageBoxButtons.YesNo);
                if (mmsgResult == MessageBoxResult.Yes)
                {
                    VM!.ResetClone();
                }
            }

            ToggleLoadingOverlay(false);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the app settings and runs startup validations
        /// </summary>
        private async void InitializeApp()
        {
            VM!.InitializeAppSettings();
            VM?.SetLoadingMessage("Validating system requirements...");

            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(2000);
            });

            if (VM!.AppSettings!.JavaPath.IsNullOrEmptyOrWhiteSpace() || !VM!.AppSettings!.JavaPath!.PathExists().Exist)
            {
                await Message.ShowError("Java was not detected on this system. \n \nPlease install Java 8 or later and set the path on the settings page.", "Java Required!", appendSuffix: false);
                PassedPrereqs = false;
            }

            if (VM!.AppSettings!.WeaponIconTemplatePath.IsNullOrEmptyOrWhiteSpace() || !VM!.AppSettings!.WeaponIconTemplateResolvedPath!.PathExists().Exist)
            {
                await Message.ShowError("Unable to find CCSUP_CustomWeaponTemplate.swf file.\n\nPlease download it from Nexus and set the path on the settings page.", "Weapon Icon Template Required!", appendSuffix: false);
                PassedPrereqs = false;
            }

            if (VM!.AppSettings!.FfdecPath.IsNullOrEmptyOrWhiteSpace() || !VM!.AppSettings!.FfdecResolvedPath!.PathExists().Exist)
            {
                await Message.ShowError("Unable to find FFDec install.\n\nPlease download FFDec v25.1.3 nightly build 3471\nor later from GitHub and set the path on the settings page.", "FFDec Required!", appendSuffix: false);
                PassedPrereqs = false;
            }

            VM?.NotifyValidationChanged();
            if (!PassedPrereqs)
            {
                VM?.SetSelectedTab(tcPages.ItemCount - 1);
            }

            VM?.SetLoadingMessage("Ready!");
            VM!.MainWindowContext = this;
            LoadingOverlay.IsVisible = false;
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Updates the SVG preview element size
        /// </summary>
        private void UpdatePreview()
        {
            if (VM.IsNullOrEmpty()) return;

            int width = (int)Math.Max(SvgDropControl.Bounds.Width, 1);
            int height = (int)Math.Max(SvgDropControl.Bounds.Height, 1);

            VM.LoadSvgPreview(width, height);
        }

        /// <summary>
        /// Toggles the loading panel overlay for a friendly way to show work being done
        /// </summary>
        /// <param name="isVisible"></param>
        /// <param name="message"></param>
        private static void ToggleLoadingOverlay(bool isVisible, string? message = null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.LoadingOverlay.IsVisible = isVisible;
                    if (!mainWindow.VM.IsNullOrEmpty() && !message.IsNullOrEmptyOrWhiteSpace())
                    {
                        if (message!.IsRegexMatch("(Clone)?FinalFilePath"))
                        {
                            switch (message)
                            {
                                case "FinalFilePath":
                                    message = $"Creating: {mainWindow.VM.FinalFilePath!.GetFileName()}";
                                    break;
                                case "CloneFinalFilePath":
                                    message = $"Cloning: {mainWindow.VM.CloneSourcePath!.GetFileName()}\nTo: {mainWindow.VM.CloneFinalFilePath!.GetFileName()}";
                                    break;
                            }
                        }
                        mainWindow.VM.SetLoadingMessage(message!);
                    }
                }
            });
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Converts the passed fullpath to a relative value using the built in %AppPath% value if the passed path exists inside the app location
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private static string ConvertToAppPath(string fullPath)
        {
            if (AppConfig.LocalPath.IsNullOrEmpty()) return fullPath;
            if (fullPath.IsRegexMatch(AppConfig.LocalPath.RegexEscape()))
            {
                return fullPath.RegexReplace(AppConfig.LocalPath.RegexEscape(), "%AppPath%");
            }
            return fullPath;
        }

        #endregion

    }
}