using Avalonia.Data.Converters;
using StarfieldWeaponIconFileBuilder.ViewModels;
using System;
using System.Globalization;

namespace StarfieldWeaponIconFileBuilder.Converters;

public class HeaderPropertyConverter : IValueConverter
{
    #region Methods

    /// <summary>
    /// Convert the tab header binding
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is TabHeaderVM header && parameter is string prop)
        {
            return prop switch
            {
                "IconPath" => header.IconPath,
                "HeaderText" => header.HeaderText,
                "Foreground" => header.Foreground,
                _ => header.ToString()
            };
        }
        return null;
    }

    /// <summary>
    /// Convert back is not implemented as the tab header is read-only in this context
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture) => throw new NotImplementedException();

    #endregion
}