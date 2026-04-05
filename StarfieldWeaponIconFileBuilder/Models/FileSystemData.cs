using System;
using System.ComponentModel;

namespace StarfieldWeaponIconFileBuilder.Models;

public class FileSystemData : INotifyPropertyChanged
{
    #region Enum

    public enum HashAlgorithmType
    {
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }

    #endregion

    #region Properties

    private Type? pType = null;
    public Type? Type
    {
        get { return pType; }
        set
        {
            if (value != pType)
            {
                pType = value;
                NotifyPropertyChanged(this, nameof(Type));
            }
        }
    }

    private bool pExist = false;
    public bool Exist
    {
        get { return pExist; }
        set
        {
            if (value != pExist)
            {
                pExist = value;
                NotifyPropertyChanged(this, nameof(Exist));
            }
        }
    }

    private string? pHash;
    public string? Hash
    {
        get { return pHash; }
        set
        {
            if (value != pHash)
            {
                pHash = value;
                NotifyPropertyChanged(this, nameof(Hash));
            }
        }
    }

    private HashAlgorithmType pAlgorithm;
    public HashAlgorithmType Algorithm
    {
        get { return pAlgorithm; }
        set
        {
            if (value != pAlgorithm)
            {
                pAlgorithm = value;
                NotifyPropertyChanged(this, nameof(Algorithm));
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
