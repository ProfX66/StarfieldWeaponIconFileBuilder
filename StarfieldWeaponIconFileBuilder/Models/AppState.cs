using System.ComponentModel;

namespace StarfieldWeaponIconFileBuilder.Models;

public class AppState : INotifyPropertyChanged
{
    #region Properties

    private bool? pInitializing = false;
    public bool? Initializing
    {
        get => pInitializing;
        set
        {
            if (value != pInitializing)
            {
                pInitializing = value;
                NotifyPropertyChanged(this, nameof(Initializing));
            }
        }
    }

    private bool? pInitialized = false;
    public bool? Initialized
    {
        get => pInitialized;
        set
        {
            if (value != pInitialized)
            {
                pInitialized = value;
                NotifyPropertyChanged(this, nameof(Initialized));
            }
        }
    }

    private bool? pContentRendered = false;
    public bool? ContentRendered
    {
        get => pContentRendered;
        set
        {
            if (value != pContentRendered)
            {
                pContentRendered = value;
                NotifyPropertyChanged(this, nameof(ContentRendered));
            }
        }
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
