using Avalonia.Media;

namespace StarfieldWeaponIconFileBuilder.Utilities;

/// <summary>
/// Common brushes
/// </summary>
public static class AppBrushes
{
    #region Properties

    public static IBrush ConstellationWhiteBrush => new SolidColorBrush(Color.Parse("#F4F5F7"));
    public static IBrush ConstellationRedBrush => new SolidColorBrush(Color.Parse("#C72138"));
    public static IBrush ConstellationOrangeBrush => new SolidColorBrush(Color.Parse("#E06236"));
    public static IBrush ConstellationGoldBrush => new SolidColorBrush(Color.Parse("#D7A64B"));
    public static IBrush ConstellationLightBlueBrush => new SolidColorBrush(Color.Parse("#4A658E"));
    public static IBrush ConstellationBlueBrush => new SolidColorBrush(Color.Parse("#304C7A"));
    public static IBrush ConstellationDarkBlueBrush => new SolidColorBrush(Color.Parse("#213150"));
    public static IBrush ControlValidationPositiveBrush => Brushes.Transparent;
    public static IBrush ControlValidationNegativeBrush => Brushes.DarkRed;

    #endregion
}
