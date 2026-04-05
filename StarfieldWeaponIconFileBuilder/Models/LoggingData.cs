using System.ComponentModel;

namespace StarfieldWeaponIconFileBuilder.Models;

public class LoggingData : INotifyPropertyChanged
{
    #region Properties

    #region Log Config

    private string? pPath;
    public string? Path
    {
        get { return pPath; }
        set
        {
            if (value != pPath)
            {
                pPath = value;
                NotifyPropertyChanged(this, nameof(Path));
            }
        }
    }

    private string? pPathPrepend;
    public string? PathPrepend
    {
        get { return pPathPrepend; }
        set
        {
            if (value != pPathPrepend)
            {
                pPathPrepend = value;
                NotifyPropertyChanged(this, nameof(PathPrepend));
            }
        }
    }

    private string? pPathAppend;
    public string? PathAppend
    {
        get { return pPathAppend; }
        set
        {
            if (value != pPathAppend)
            {
                pPathAppend = value;
                NotifyPropertyChanged(this, nameof(PathAppend));
            }
        }
    }

    private string? pFileName;
    public string? FileName
    {
        get { return pFileName; }
        set
        {
            if (value != pFileName)
            {
                pFileName = value;
                NotifyPropertyChanged(this, nameof(FileNameAppend));
            }
        }
    }

    private string? pFileNameAppend;
    public string? FileNameAppend
    {
        get { return pFileNameAppend; }
        set
        {
            if (value != pFileNameAppend)
            {
                pFileNameAppend = value;
                NotifyPropertyChanged(this, nameof(FileNameAppend));
            }
        }
    }

    private bool? pInstanceRoll = false;
    public bool? InstanceRoll
    {
        get { return pInstanceRoll; }
        set
        {
            if (value != pInstanceRoll)
            {
                pInstanceRoll = value;
                NotifyPropertyChanged(this, nameof(InstanceRoll));
            }
        }
    }

    private int? pMaxCount = 9;
    public int? MaxCount
    {
        get { return pMaxCount; }
        set
        {
            if (value != pMaxCount)
            {
                pMaxCount = value;
                NotifyPropertyChanged(this, nameof(MaxCount));
            }
        }
    }

    private string? pMaxSize = "5 MB";
    public string? MaxSize
    {
        get { return pMaxSize; }
        set
        {
            if (value != pMaxSize)
            {
                pMaxSize = value;
                NotifyPropertyChanged(this, nameof(MaxSize));
            }
        }
    }

    private string? pEventSource = "Application";
    public string? EventSource
    {
        get { return pEventSource; }
        set
        {
            if (value != pEventSource)
            {
                pEventSource = value;
                NotifyPropertyChanged(this, nameof(EventSource));
            }
        }
    }

    private bool? pInteractive = false;
    public bool? Interactive
    {
        get { return pInteractive; }
        set
        {
            if (value != pInteractive)
            {
                pInteractive = value;
                NotifyPropertyChanged(this, nameof(Interactive));
            }
        }
    }

    private bool? pInteractiveDetection = true;
    public bool? InteractiveDetection
    {
        get { return pInteractiveDetection; }
        set
        {
            if (value != pInteractiveDetection)
            {
                pInteractiveDetection = value;
                NotifyPropertyChanged(this, nameof(InteractiveDetection));
            }
        }
    }

    private bool? pUseLocalAppData = true;
    public bool? UseLocalAppData
    {
        get { return pUseLocalAppData; }
        set
        {
            if (value != pUseLocalAppData)
            {
                pUseLocalAppData = value;
                NotifyPropertyChanged(this, nameof(UseLocalAppData));
            }
        }
    }

    #endregion

    #region Log Level

    private bool? pVerbose = false;
    public bool? Verbose
    {
        get { return pVerbose; }
        set
        {
            if (value != pVerbose)
            {
                pVerbose = value;
                NotifyPropertyChanged(this, nameof(Verbose));
            }
        }
    }

    private bool? pDebug = false;
    public bool? Debug
    {
        get { return pDebug; }
        set
        {
            if (value != pDebug)
            {
                pDebug = value;
                NotifyPropertyChanged(this, nameof(Debug));
            }
        }
    }

    private bool? pWhatIf = false;
    public bool? WhatIf
    {
        get { return pWhatIf; }
        set
        {
            if (value != pWhatIf)
            {
                pWhatIf = value;
                NotifyPropertyChanged(this, nameof(WhatIf));
            }
        }
    }

    private bool? pInformational = false;
    public bool? Informational
    {
        get { return pInformational; }
        set
        {
            if (value != pInformational)
            {
                pInformational = value;
                NotifyPropertyChanged(this, nameof(Informational));
            }
        }
    }

    private bool? pAdvisory = false;
    public bool? Advisory
    {
        get { return pAdvisory; }
        set
        {
            if (value != pAdvisory)
            {
                pAdvisory = value;
                NotifyPropertyChanged(this, nameof(Advisory));
            }
        }
    }

    private bool? pError = false;
    public bool? Error
    {
        get { return pError; }
        set
        {
            if (value != pError)
            {
                pError = value;
                NotifyPropertyChanged(this, nameof(Error));
            }
        }
    }

    #endregion

    #endregion

    #region EventHandler

    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged(object sender, string propertyName)
    {
        PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
