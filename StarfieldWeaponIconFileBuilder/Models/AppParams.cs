using System.Collections.Generic;
using System.ComponentModel;

namespace StarfieldWeaponIconFileBuilder.Models;

public class AppParams : INotifyPropertyChanged
{
    #region Properties

    private string? pFullCommandLine;
    public string? FullCommandLine
    {
        get { return pFullCommandLine; }
        set
        {
            if (value != pFullCommandLine)
            {
                pFullCommandLine = value;
                NotifyPropertyChanged(this, nameof(FullCommandLine));
            }
        }
    }

    private string? pFullCommandLineRaw;
    public string? FullCommandLineRaw
    {
        get { return pFullCommandLineRaw; }
        set
        {
            if (value != pFullCommandLineRaw)
            {
                pFullCommandLineRaw = value;
                NotifyPropertyChanged(this, nameof(FullCommandLineRaw));
            }
        }
    }

    private Dictionary<string, object>? pParameterTable;
    public Dictionary<string, object>? ParameterTable
    {
        get { return pParameterTable; }
        set
        {
            if (value != pParameterTable)
            {
                pParameterTable = value;
                NotifyPropertyChanged(this, nameof(ParameterTable));
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
