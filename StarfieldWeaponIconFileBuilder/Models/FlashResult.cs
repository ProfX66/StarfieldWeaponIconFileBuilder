using System.ComponentModel;

namespace StarfieldWeaponIconFileBuilder.Models;

public class FlashResult : INotifyPropertyChanged
{
    #region Properties

    private string? pAction;
    public string? Action
    {
        get => pAction;
        set
        {
            if (value != pAction)
            {
                pAction = value;
                NotifyPropertyChanged(nameof(Action));
            }
        }
    }

    private string? pSourcePath;
    public string? SourcePath
    {
        get => pSourcePath;
        set
        {
            if (value != pSourcePath)
            {
                pSourcePath = value;
                NotifyPropertyChanged(nameof(SourcePath));
            }
        }
    }

    private string? pDestinationPath;
    public string? DestinationPath
    {
        get => pDestinationPath;
        set
        {
            if (value != pDestinationPath)
            {
                pDestinationPath = value;
                NotifyPropertyChanged(nameof(DestinationPath));
            }
        }
    }

    private string? pExportedPath;
    public string? ExportedPath
    {
        get => pExportedPath;
        set
        {
            if (value != pExportedPath)
            {
                pExportedPath = value;
                NotifyPropertyChanged(nameof(ExportedPath));
            }
        }
    }

    private bool? pResult;
    public bool? Result
    {
        get => pResult;
        set
        {
            if (value != pResult)
            {
                pResult = value;
                NotifyPropertyChanged(nameof(Result));
            }
        }
    }

    #endregion

    #region EventHandler

    /// <summary>
    /// Property changed event
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Triggers the property changed event
    /// </summary>
    /// <param name="propertyName"></param>
    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
