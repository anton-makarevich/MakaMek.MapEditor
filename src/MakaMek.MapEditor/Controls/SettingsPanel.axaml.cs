using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Sanet.MakaMek.MapEditor.Controls;

public partial class SettingsPanel : UserControl
{
    public event EventHandler<RoutedEventArgs>? ExportPdfClicked;

    public SettingsPanel()
    {
        InitializeComponent();
        ExportPdfButton.Click += OnExportPdfButtonClicked;
    }

    private void OnExportPdfButtonClicked(object? sender, RoutedEventArgs e)
    {
        ExportPdfClicked?.Invoke(this, e);
    }
}
