using System.ComponentModel;

namespace StarfieldWeaponIconFileBuilder.Models;

public class ProcessData : INotifyPropertyChanged
{
    #region Properties

    private int? pRC;
    public int? RC
    {
        get { return pRC; }
        set
        {
            if (value != pRC)
            {
                pRC = value;
                NotifyPropertyChanged(this, nameof(RC));
            }
        }
    }

    private string? pStandardOutput;
    public string? StandardOutput
    {
        get { return pStandardOutput; }
        set
        {
            if (value != pStandardOutput)
            {
                pStandardOutput = value;
                NotifyPropertyChanged(this, nameof(StandardOutput));
            }
        }
    }

    private string? pStandardError;
    public string? StandardError
    {
        get { return pStandardError; }
        set
        {
            if (value != pStandardError)
            {
                pStandardError = value;
                NotifyPropertyChanged(this, nameof(StandardError));
            }
        }
    }

    private int? pPid;
    public int? Pid
    {
        get { return pPid; }
        set
        {
            if (value != pPid)
            {
                pPid = value;
                NotifyPropertyChanged(this, nameof(Pid));
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
