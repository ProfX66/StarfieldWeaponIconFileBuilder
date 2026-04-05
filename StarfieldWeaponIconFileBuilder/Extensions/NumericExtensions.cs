using System;
using System.Collections.Specialized;
using System.Linq;

namespace StarfieldWeaponIconFileBuilder.Extensions;
public static class NumericExtensions
{
    #region Methods

    /// <summary>
    /// Finds the largest key size in the passed dictionary
    /// </summary>
    /// <param name="Dict"></param>
    /// <param name="ExtraPadding"></param>
    /// <returns></returns>
    public static int GetLargestKeySize(this OrderedDictionary Dict, int ExtraPadding = 0)
    {
        if (Dict.IsNullOrEmpty() || Dict.Keys.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(Dict), "OrderedDictionary or its Keys collection cannot be null.");

        return Dict.Keys.Cast<object>().Max(key => key?.ToString()?.Length ?? 0) + ExtraPadding;
    }

    /// <summary>
    /// Converts a friendly data size to the real szie in bytes
    /// </summary>
    /// <param name="FriendlySize"></param>
    /// <returns>Double</returns>
    public static double ToRawSize(this string FriendlySize)
    {
        string Unit = FriendlySize.RegexReplace(@"\d", "").Trim().ToUpper();
        double Value = Convert.ToDouble(FriendlySize.RegexReplace(@"\D", ""));
        string[] Units = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
        double factor = 1.0;

        for (int i = 0; i < Units.Length; i++)
        {
            if (Unit == Units[i])
            {
                break;
            }

            factor *= 1024;
        }

        return Value *= factor;
    }

    /// <summary>
    /// Converts a friendly data size to the real szie in bytes
    /// </summary>
    /// <param name="FriendlySize"></param>
    /// <returns>long</returns>
    public static long ToRawSizeLong(this string FriendlySize)
    {
        
        string Unit = FriendlySize.RegexReplace(@"\d", "").Trim().ToUpper();
        long Value = Convert.ToInt64(FriendlySize.RegexReplace(@"\D", ""));
        string[] Units = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
        long factor = 1;

        for (int i = 0; i < Units.Length; i++)
        {
            if (Unit == Units[i])
            {
                break;
            }

            factor *= 1024;
        }

        return Value *= factor;
    }

    /// <summary>
    /// Shortcut to convert to Int32
    /// </summary>
    /// <param name="InputObject"></param>
    /// <returns>int</returns>
    public static int ToInt32(this object InputObject)
    {
        return Convert.ToInt32(InputObject);
    }

    /// <summary>
    /// Converts the passed object to a nullable Int
    /// </summary>
    /// <param name="InputObject"></param>
    /// <returns></returns>
    public static int? ToNullableInt32(this object InputObject)
    {
        if (InputObject.IsNullOrEmptyOrWhiteSpace())
            return null;

        if (InputObject is int i)
            return i;

        if (int.TryParse(InputObject.ToString(), out int result))
            return result;

        return null;
    }

    /// <summary>
    /// Returns the positive version of the passed int
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int ToPositive(this int value)
    {
        if (value == int.MinValue)
            return int.MaxValue;

        return value < 0 ? -value : value;
    }

    /// <summary>
    /// Returns the negative version of the passed int
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int ToNegative(this int value)
    {
        if (value == int.MinValue)
            return int.MinValue;

        return value > 0 ? -value : value;
    }

    #endregion
}
