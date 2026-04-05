using Newtonsoft.Json;
using StarfieldWeaponIconFileBuilder.Extensions;
using StarfieldWeaponIconFileBuilder.Utilities;
using System;
using System.ComponentModel;
using System.IO;

namespace StarfieldWeaponIconFileBuilder.Models;

public class AppSettings : INotifyPropertyChanged
{
    #region Properties

    private static bool _IsInitialized = false;

    private string? _WeaponIconTemplatePath;
    [JsonProperty]
    public string? WeaponIconTemplatePath
    {
        get { return _WeaponIconTemplatePath; }
        set
        {
            if (value != _WeaponIconTemplatePath)
            {
                _WeaponIconTemplatePath = value;
                WeaponIconTemplateResolvedPath = value!.ExpandVariables();
                NotifyPropertyChanged(this, nameof(WeaponIconTemplatePath));
            }
        }
    }

    private string? _WeaponIconTemplateResolvedPath;
    [JsonIgnore]
    public string? WeaponIconTemplateResolvedPath
    {
        get { return _WeaponIconTemplateResolvedPath; }
        set
        {
            if (value != _WeaponIconTemplateResolvedPath)
            {
                _WeaponIconTemplateResolvedPath = value;
                Starfield.TemplatePath = value;
                NotifyPropertyChanged(this, nameof(WeaponIconTemplateResolvedPath));
            }
        }
    }

    private string? _FfdecPath;
    [JsonProperty]
    public string? FfdecPath
    {
        get { return _FfdecPath; }
        set
        {
            if (value != _FfdecPath)
            {
                _FfdecPath = value;
                FfdecResolvedPath = value!.ExpandVariables();
                NotifyPropertyChanged(this, nameof(FfdecPath));
            }
        }
    }

    private string? _FfdecResolvedPath;
    [JsonIgnore]
    public string? FfdecResolvedPath
    {
        get { return _FfdecResolvedPath; }
        set
        {
            if (value != _FfdecResolvedPath)
            {
                _FfdecResolvedPath = value;
                Flash.FfdecPath = value;
                NotifyPropertyChanged(this, nameof(FfdecResolvedPath));
            }
        }
    }

    private string? _JavaPath;
    [JsonProperty]
    public string? JavaPath
    {
        get { return _JavaPath; }
        set
        {
            if (value != _JavaPath)
            {
                _JavaPath = value;
                Flash.JavaPath = value;
                NotifyPropertyChanged(this, nameof(JavaPath));
            }
        }
    }

    private string? _TemplateClassName;
    [JsonProperty]
    public string? TemplateClassName
    {
        get { return _TemplateClassName; }
        set
        {
            if (value != _TemplateClassName)
            {
                _TemplateClassName = value;
                Save();
                Starfield.TemplateClassName = value;
                NotifyPropertyChanged(this, nameof(TemplateClassName));
            }
        }
    }

    private string? _TemplatePrefix;
    [JsonProperty]
    public string? TemplatePrefix
    {
        get { return _TemplatePrefix; }
        set
        {
            if (value != _TemplatePrefix)
            {
                _TemplatePrefix = value;
                Save();
                Starfield.FilePrefix = value;
                NotifyPropertyChanged(this, nameof(TemplatePrefix));
            }
        }
    }

    private bool _AutoResizeIcon = true;
    [JsonProperty]
    public bool AutoResizeIcon
    {
        get { return _AutoResizeIcon; }
        set
        {
            if (value != _AutoResizeIcon)
            {
                _AutoResizeIcon = value;
                Save();
                NotifyPropertyChanged(this, nameof(AutoResizeIcon));
            }
        }
    }

    private bool _VerboseLogging = false;
    [JsonProperty]
    public bool VerboseLogging
    {
        get { return _VerboseLogging; }
        set
        {
            if (value != _VerboseLogging)
            {
                _VerboseLogging = value;
                Save();
                NotifyPropertyChanged(this, nameof(VerboseLogging));
            }
        }
    }

    private string? _SettingsFilePath;
    [JsonIgnore]
    public string? SettingsFilePath
    {
        get { return _SettingsFilePath; }
        set
        {
            if (value != _SettingsFilePath)
            {
                _SettingsFilePath = value;
                SettingsResolvedFilePath = value!.ExpandVariables();
                NotifyPropertyChanged(this, nameof(SettingsFilePath));
            }
        }
    }

    private string? _SettingsResolvedFilePath;
    [JsonIgnore]
    public string? SettingsResolvedFilePath
    {
        get { return _SettingsResolvedFilePath; }
        set
        {
            if (value != _SettingsResolvedFilePath)
            {
                _SettingsResolvedFilePath = value;
                NotifyPropertyChanged(this, nameof(SettingsResolvedFilePath));
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Save settings to JSON file.
    /// </summary>
    public void Save(bool force = false)
    {
        if (!_IsInitialized && !force) return;

        try
        {
            var folder = SettingsResolvedFilePath!.GetDirectoryName();
            if (folder.IsNullOrEmptyOrWhiteSpace())
                throw new DirectoryNotFoundException($"Unable to determine directory for settings file from path: {SettingsResolvedFilePath}");
            if (!folder.PathExists().Exist)
                folder.TryCreateDirectory();

            Logging.Informational($"Saving settings to: {SettingsResolvedFilePath}");
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(SettingsResolvedFilePath!, json);

        }
        catch (Exception Ex)
        {
            Message.ShowError($"Failed to save application settings to: {SettingsResolvedFilePath}", exception: Ex).Wait();
        }
    }

    /// <summary>
    /// Load settings from JSON file. Use defaults if file does not exist or fails to load. Save defaults to file if file does not exist.
    /// </summary>
    public void Load()
    {
        GetDefaults();
        if (!SettingsResolvedFilePath!.PathExists().Exist)
        {
            Save(true);
            return;
        }

        string json = File.ReadAllText(SettingsResolvedFilePath!);
        AppSettings? loadedSettings = JsonConvert.DeserializeObject<AppSettings>(json);
        if (!loadedSettings.IsNullOrEmpty())
        {
            if (!AppConfig.ApplicationParameters!.ParameterTable!.ContainsKey("WeaponIconTemplatePath"))
            {
                WeaponIconTemplatePath = loadedSettings.WeaponIconTemplatePath;
            }

            if (!AppConfig.ApplicationParameters!.ParameterTable!.ContainsKey("FfdecPath"))
            {
                FfdecPath = loadedSettings.FfdecPath;
            }

            if (!AppConfig.ApplicationParameters!.ParameterTable!.ContainsKey("JavaPath"))
            {
                JavaPath = loadedSettings.JavaPath;
            }

            if (!AppConfig.ApplicationParameters!.ParameterTable!.ContainsKey("TemplateClassName"))
            {
                TemplateClassName = loadedSettings.TemplateClassName;
            }

            if (!AppConfig.ApplicationParameters!.ParameterTable!.ContainsKey("TemplatePrefix"))
            {
                TemplatePrefix = loadedSettings.TemplatePrefix;
            }

            if (!AppConfig.ApplicationParameters!.ParameterTable!.ContainsKey("AutoResizeIcon"))
            {
                AutoResizeIcon = loadedSettings.AutoResizeIcon;
            }

            if (!AppConfig.ApplicationParameters!.ParameterTable!.ContainsKey("Verbose"))
            {
                VerboseLogging = loadedSettings.VerboseLogging;
                Logging.LogConfig!.Verbose = VerboseLogging;
                Logging.Verbose("Verbose logging enabled by the settings file...");
            }
        }

        Logging.Informational($"Loaded Settings:\n{json}");
        Logging.None();

        _IsInitialized = true;
    }

    /// <summary>
    /// Initializes configuration properties with default values
    /// </summary>
    public void GetDefaults()
    {
        SettingsFilePath = AppConfig.GetParamValue("SettingsFilePath", @"%AppPath%\StarfieldWeaponIconFileBuilder.settings.json")!.ToString();
        WeaponIconTemplatePath = AppConfig.GetParamValue("WeaponIconTemplatePath", @"%AppPath%\Assets\CCSUP_CustomWeaponTemplate.swf")!.ToString();
        FfdecPath = AppConfig.GetParamValue("FfdecPath", @"%AppPath%\FFDec\ffdec-cli.jar")!.ToString();
        TemplateClassName = AppConfig.GetParamValue("TemplateClassName", "CustomWeaponTemplate")!.ToString();
        TemplatePrefix = AppConfig.GetParamValue("TemplatePrefix", "CCSUP")!.ToString();
        AutoResizeIcon = AppConfig.GetParamValue("AutoResizeIcon", "true")!.ToBoolean();
        VerboseLogging = Logging.LogConfig!.Verbose.GetValueOrDefault();
        FileInfo? javaPath = "Java.exe".FindExecutable();
        if (!javaPath.IsNullOrEmptyOrWhiteSpace())
        {
            JavaPath = javaPath.FullName;
        }
    }

    /// <summary>
    /// Resets all settings to their default values and saves to file.
    /// </summary>
    public void ResetToDefaults()
    {
        GetDefaults();
        Save();
    }

    #endregion

    #region EventHandler

    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged(object sender, string propertyName)
    {
        PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
