using Avalonia.Controls;
using Avalonia.Input;
using ETC.Desktop.ViewModels;

namespace ETC.Desktop.Views;

public partial class ShellView : UserControl
{
    public ShellView() => InitializeComponent();

    // Fokus hilang bersama layar masuk; tanpa ini shortcut keyboard belum aktif.
    protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Focus();
    }

    void OnScrimPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ShellViewModel vm) vm.CloseSheetCommand.Execute(null);
    }
}
