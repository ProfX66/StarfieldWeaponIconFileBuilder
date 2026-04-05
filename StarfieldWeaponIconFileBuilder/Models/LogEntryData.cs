using System.ComponentModel;

namespace StarfieldWeaponIconFileBuilder.Models;

public class LogEntryData : INotifyPropertyChanged
{
    #region Properties

    private string? pText = string.Empty;
    public string? Text
    {
        get { return pText; }
        set
        {
            if (value != pText)
            {
                pText = value;
                NotifyPropertyChanged(this, nameof(Text));
            }
        }
    }

    private bool? pAppend = true;
    public bool? Append
    {
        get { return pAppend; }
        set
        {
            if (value != pAppend)
            {
                pAppend = value;
                NotifyPropertyChanged(this, nameof(Append));
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