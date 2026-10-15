using Avalonia.Controls;
using Avalonia.Input;

namespace ETC.Desktop.Views;

public partial class ExploreView : UserControl
{
    public ExploreView() => InitializeComponent();

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            SearchBox.Focus();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }
}
