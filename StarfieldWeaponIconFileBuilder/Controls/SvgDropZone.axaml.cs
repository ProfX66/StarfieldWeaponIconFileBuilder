using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using StarfieldWeaponIconFileBuilder.Extensions;
using StarfieldWeaponIconFileBuilder.ViewModels;
using System;
using System.Linq;

namespace StarfieldWeaponIconFileBuilder;

public partial class SvgDropZone : UserControl
{
    #region Properties

    private MainWindowViewModel? VM => DataContext as MainWindowViewModel;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public SvgDropZone()
    {
        InitializeComponent();
    }

    #endregion

    #region Events

    /// <summary>
    /// Update the drag effects to show copy if a file is being dragged, otherwise show none
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer?.Formats.Contains(DataFormat.File) == true)
            e.DragEffects = DragDropEffects.Copy;
        else
            e.DragEffects = DragDropEffects.None;
    }

    /// <summary>
    /// Load and render the dropped SVG file, then reset the border/background to default values
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.IsNullOrEmpty() || VM.IsNullOrEmpty())
            return;

        IStorageItem[]? files = e.DataTransfer.TryGetFiles();
        IStorageFile? svgFile = files?.OfType<IStorageFile>().FirstOrDefault(f => f.Name.EndsWith(".svg", StringComparison.OrdinalIgnoreCase));

        if (!svgFile.IsNullOrEmpty())
        {
            VM.SvgPath = svgFile.Path.LocalPath;
            int width = (int)Math.Max(DropZone.Bounds.Width, 1);
            int height = (int)Math.Max(DropZone.Bounds.Height, 1);
            VM.LoadSvgPreview(width, height);
            VM.BorderStroke = VM.DefaultBorderStroke;
            VM.BackgroundColor = Brushes.Transparent;
        }
    }

    /// <summary>
    /// Sets the border and background to gold to indicate that the dragged item can be dropped here
    /// </summary>
    /// <param name="s"></param>
    /// <param name="e"></param>
    private void OnDragEnter(object s, DragEventArgs e)
    {
        if (VM.IsNullOrEmpty()) return;
        VM.BorderStroke = VM.ConstellationGold;
        VM.BackgroundColor = VM.ConstellationGold;
    }

    /// <summary>
    /// Sets the border and background back to default values when the dragged item leaves the drop zone without dropping
    /// </summary>
    /// <param name="s">The event source.</param>
    /// <param name="e">The drag event data.</param>
    private void OnDragLeave(object s, DragEventArgs e)
    {
        if (VM.IsNullOrEmpty()) return;
        VM.BorderStroke = VM.DefaultBorderStroke;
        VM.BackgroundColor = Brushes.Transparent;
    }

    #endregion

}