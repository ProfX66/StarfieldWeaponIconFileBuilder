using System.ComponentModel;
using CsvHelper.Configuration;

namespace StarfieldWeaponIconFileBuilder.Models;

public class FlashSymbolMap : INotifyPropertyChanged
{
    #region Properties

    private int? pId;
    public int? Id
    {
        get => pId;
        set
        {
            if (value != pId)
            {
                pId = value;
                NotifyPropertyChanged(nameof(Id));
            }
        }
    }

    private string? pSymbol;
    public string? Symbol
    {
        get => pSymbol;
        set
        {
            if (value != pSymbol)
            {
                pSymbol = value;
                NotifyPropertyChanged(nameof(Symbol));
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

/// <summary>
/// CSV Class mapping
/// </summary>
public sealed class MappedFlashSymbols : ClassMap<FlashSymbolMap>
{
    public MappedFlashSymbols()
    {
        Map(m => m.Id).Index(0);
        Map(m => m.Symbol).Index(1);
    }
}
