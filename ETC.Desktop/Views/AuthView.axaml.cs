using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ETC.Desktop.Views;

public partial class AuthView : UserControl
{
    public AuthView() => InitializeComponent();

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        LoginIdBox.Focus();
    }
}
