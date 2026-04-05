using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CustomMessageBox.Avalonia;
using StarfieldWeaponIconFileBuilder.Extensions;
using StarfieldWeaponIconFileBuilder.Models;
using StarfieldWeaponIconFileBuilder.Utilities;

namespace StarfieldWeaponIconFileBuilder.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #region Properties

    #region Application

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ResetSettingsCommand))]
    private AppSettings? appSettings;
    [ObservableProperty]
    private bool nonSettingTabState = true;

    public bool JavaPathInvalid => !ValidatePathValid("JavaPath");
    public bool WeaponIconTemplateInvalid => !ValidatePathValid("WeaponIconTemplatePath");
    public bool FfdecPathInvalid => !ValidatePathValid("FfdecPath");

    /// <summary>
    /// Validates that the passed setting path is valid and exists
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private bool ValidatePathValid(string name)
    {
        bool returnValue = false;
        if (name.IsNullOrEmptyOrWhiteSpace()) return returnValue;
        if (AppSettings.IsNullOrEmpty()) return returnValue;

        switch (name)
        {
            case "JavaPath":
                returnValue = !AppSettings!.JavaPath.IsNullOrEmptyOrWhiteSpace() && AppSettings.JavaPath.PathExists().Exist;
                break;
            case "WeaponIconTemplatePath":
                returnValue = !AppSettings!.WeaponIconTemplateResolvedPath.IsNullOrEmptyOrWhiteSpace() && AppSettings.WeaponIconTemplateResolvedPath.PathExists().Exist;
                break;
            case "FfdecPath":
                returnValue = !AppSettings!.FfdecResolvedPath.IsNullOrEmptyOrWhiteSpace() && AppSettings.FfdecResolvedPath.PathExists().Exist;
                break;
        }

        return returnValue;
    }

    #endregion


    #region Constants

    private const string DefaultCreateButtonText = "Build: Custom Weapon Icon File";
    private const string DefaultCloneButtonText = "Copy to: Custom Weapon Icon File";

    #endregion


    #region Read-Only Expression Properties

    public bool ShowPlaceholder => SvgPreview.IsNullOrEmpty();
    public bool HasSvg => !string.IsNullOrEmpty(SvgPath);
    public bool IsValid => ValidateItems();
    public bool IsCloneValid => ValidateCloneItems();
    public bool SaveSettings => SaveSettingsFile();

    #endregion


    #region Commands

    public IRelayCommand ResetCreateCommand { get; }
    public IRelayCommand ResetSvgCommand { get; }
    public IRelayCommand ResetCLoneCommand { get; }
    public IRelayCommand ResetSettingsCommand { get; }

    #endregion


    #region Creation Observables

    [ObservableProperty]
    private string? finalFilePath;
    [ObservableProperty]
    private string? createButtonText = DefaultCreateButtonText;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSvg))]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(ResetCreateCommand))]
    private string? weaponLinkageName;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSvg))]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(ResetCreateCommand))]
    private string? exportPath;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSvg))]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(ResetCreateCommand))]
    private string? svgPath;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPlaceholder))]
    [NotifyPropertyChangedFor(nameof(HasSvg))]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    [NotifyCanExecuteChangedFor(nameof(ResetSvgCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetCreateCommand))]
    private Bitmap? svgPreview;

    #endregion


    #region Cloning Observables

    [ObservableProperty]
    private string? cloneFinalFilePath;
    [ObservableProperty]
    private string? cloneCreateButtonText = DefaultCloneButtonText;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCloneValid))]
    [NotifyCanExecuteChangedFor(nameof(ResetCLoneCommand))]
    private string? cloneWeaponLinkageName;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCloneValid))]
    [NotifyCanExecuteChangedFor(nameof(ResetCLoneCommand))]
    private string? cloneSourcePath;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCloneValid))]
    [NotifyCanExecuteChangedFor(nameof(ResetCLoneCommand))]
    private string? cloneExportPath;

    #endregion

    #endregion


    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public MainWindowViewModel()
    {
        ResetSvgCommand = new RelayCommand(ResetSvg, () => HasSvg);
        ResetCreateCommand = new RelayCommand(ResetCreate, () => IsValid);
        ResetCLoneCommand = new RelayCommand(ResetClone, () => IsCloneValid);
        ResetSettingsCommand = new RelayCommand(ResetSettings, () => !AppSettings.IsNullOrEmpty());
    }

    #endregion


    #region Methods

    #region Property Validation
    /// <summary>
    /// Validates required properties for icon file creation
    /// </summary>
    /// <returns></returns>
    private bool ValidateItems()
    {
        if (WeaponLinkageName.IsNullOrEmptyOrWhiteSpace()) return false;
        if (ExportPath.IsNullOrEmptyOrWhiteSpace()) return false;
        string tempLinkageName = !WeaponLinkageName.IsNullOrEmpty() && WeaponLinkageName!.IsRegexMatch(@"^CCSUP") ? $"{WeaponLinkageName}.swf" : $"CCSUP_{WeaponLinkageName}.swf";
        CreateButtonText = $"Build: {tempLinkageName.Replace("_", "__")}";
        FinalFilePath = ExportPath!.AppendPath(tempLinkageName);
        return HasSvg;
    }

    /// <summary>
    /// Validates required properties for cloning an icon file
    /// </summary>
    /// <returns></returns>
    private bool ValidateCloneItems()
    {
        if (CloneWeaponLinkageName.IsNullOrEmptyOrWhiteSpace()) return false;
        if (CloneExportPath.IsNullOrEmptyOrWhiteSpace()) return false;
        string tempLinkageName = !CloneWeaponLinkageName.IsNullOrEmpty() && CloneWeaponLinkageName!.IsRegexMatch(@"^CCSUP") ? $"{CloneWeaponLinkageName}.swf" : $"CCSUP_{CloneWeaponLinkageName}.swf";
        CloneCreateButtonText = $"Copy to: {tempLinkageName.Replace("_", "__")}";
        CloneFinalFilePath = CloneExportPath!.AppendPath(tempLinkageName);
        return true;
    }

    #endregion

    #region Reset Properties

    /// <summary>
    /// Clears the loaded SVG file and its rendered preview
    /// </summary>
    private void ResetSvg()
    {
        SvgPath = null;
        SvgPreview = null;
        BorderStroke = DefaultBorderStroke;
        OnPropertyChanged(nameof(ShowPlaceholder));
        OnPropertyChanged(nameof(HasSvg));
    }

    /// <summary>
    /// Resets the create properties
    /// </summary>
    public void ResetCreate()
    {
        ResetSvg();
        WeaponLinkageName = null;
        ExportPath = null;
        CreateButtonText = DefaultCreateButtonText;
        OnPropertyChanged(nameof(WeaponLinkageName));
        OnPropertyChanged(nameof(ExportPath));
        OnPropertyChanged(nameof(CreateButtonText));
    }

    /// <summary>
    /// Resets the clone properties
    /// </summary>
    public void ResetClone()
    {
        CloneWeaponLinkageName = null;
        CloneSourcePath = null;
        CloneExportPath = null;
        CloneCreateButtonText = DefaultCloneButtonText;
        OnPropertyChanged(nameof(CloneWeaponLinkageName));
        OnPropertyChanged(nameof(CloneSourcePath));
        OnPropertyChanged(nameof(CloneExportPath));
        OnPropertyChanged(nameof(CloneCreateButtonText));
    }

    #endregion

    #region SVG

    /// <summary>
    /// Renders the SvgPath if not null to the passed width and height
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void LoadSvgPreview(int width, int height)
    {
        if (!string.IsNullOrEmpty(SvgPath))
        {
            SvgPreview = SvgRenderer.Render(SvgPath, width, height);
            BorderStroke = DefaultBorderStroke;
        }
    }

    #endregion

    #region Settings

    /// <summary>
    /// Initialize application settings
    /// </summary>
    public void InitializeAppSettings()
    {
        AppSettings = new AppSettings();
        AppSettings.Load();
        Logging.Informational($"JavaPath: {AppSettings.JavaPath}");
        Logging.Informational($"WeaponIconTemplatePath: {AppSettings.WeaponIconTemplateResolvedPath}");
        Logging.Informational($"FfdecPath: {AppSettings.FfdecResolvedPath}");
        Logging.Informational($"TemplateClassName: {AppSettings.TemplateClassName}");
        Logging.Informational($"TemplatePrefix: {AppSettings.TemplatePrefix}");
        Logging.Informational($"AutoResizeIcon: {AppSettings.AutoResizeIcon}");
        Logging.None();

        AppSettings.PropertyChanged += (s, e) =>
        {
            NotifyValidationChanged();
        };
    }

    /// <summary>
    /// Validates the settings and saves to disk and enables UI elements
    /// </summary>
    public void NotifyValidationChanged()
    {
        OnPropertyChanged(nameof(JavaPathInvalid));
        OnPropertyChanged(nameof(WeaponIconTemplateInvalid));
        OnPropertyChanged(nameof(FfdecPathInvalid));

        if (!JavaPathInvalid && !WeaponIconTemplateInvalid && !FfdecPathInvalid)
        {
            AppSettings?.Save();
            NonSettingTabState = true;
        }
        else
        {
            NonSettingTabState = false;
        }
    }

    /// <summary>
    /// Save current settings to disk
    /// </summary>
    /// <returns></returns>
    private bool SaveSettingsFile()
    {
        if (AppSettings.IsNullOrEmpty()) return false;
        AppSettings.Save();
        return true;
    }

    /// <summary>
    /// Reset settings to defaults
    /// </summary>
    private async void ResetSettings()
    {
        MessageBoxResult msgResult = await Message.Show("This will reset all settings back to defaults. \n \nYou will lose any changes, continue?", "Are you sure?", MessageBoxIcon.Question, MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
        if (msgResult != MessageBoxResult.Yes) return;

        AppSettings!.ResetToDefaults();
        OnPropertyChanged(nameof(AppSettings));
        OnPropertyChanged(nameof(JavaPathInvalid));
        OnPropertyChanged(nameof(WeaponIconTemplateInvalid));
        OnPropertyChanged(nameof(FfdecPathInvalid));

        Logging.Informational($"JavaPath: {AppSettings.JavaPath}");
        Logging.Informational($"WeaponIconTemplatePath: {AppSettings.WeaponIconTemplateResolvedPath}");
        Logging.Informational($"FfdecPath: {AppSettings.FfdecResolvedPath}");
        Logging.Informational($"TemplateClassName: {AppSettings.TemplateClassName}");
        Logging.Informational($"TemplatePrefix: {AppSettings.TemplatePrefix}");
        Logging.Informational($"AutoResizeIcon: {AppSettings.AutoResizeIcon}");
        Logging.None();
    }

    #endregion

    #region UI Helpers

    /// <summary>
    /// Append the passed string to the current window title property
    /// </summary>
    /// <param name="title"></param>
    public void AppendWindowTitle(string title)
    {
        WindowTitle = $"{WindowTitle} {title}";
        OnPropertyChanged(nameof(WindowTitle));
    }

    /// <summary>
    /// Sets the current tab programatically
    /// </summary>
    /// <param name="index"></param>
    public void SetSelectedTab(int index)
    {
        SelectedTabIndex = index;
        OnPropertyChanged(nameof(SelectedTabIndex));
    }

    /// <summary>
    /// Updates the loading panel title text
    /// </summary>
    /// <param name="message"></param>
    public void SetLoadingTitle(string message)
    {
        Logging.Informational($"Setting LoadingTitle to: {message}");
        LoadingTitle = message;
        OnPropertyChanged(nameof(LoadingTitle));
    }

    /// <summary>
    /// Updates the loading panel message text
    /// </summary>
    /// <param name="message"></param>
    public void SetLoadingMessage(string message)
    {
        Logging.Informational($"Setting LoadingMessage to: {message}");
        LoadingMessage = message;
        OnPropertyChanged(nameof(LoadingMessage));
    }

    #endregion

    #endregion

}
