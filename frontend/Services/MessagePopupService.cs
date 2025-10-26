using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

public static class MessagePopupService
{
    public static async Task ShowPopupAsync<TView, TViewModel>(Window owner, TViewModel viewModel)
        where TView : UserControl, new()
        where TViewModel : class
    {
        var originalEffect = owner.Content is Control content ? content.Effect : null;

        if (owner.Content is Control ownerContent)
            ownerContent.Effect = new Avalonia.Media.BlurEffect{ Radius = 10 };

        var popup = new TView { DataContext = viewModel };
        var popupWindow = new Window
        {
            Content = popup,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Width = owner?.Width ?? 412,
            Height = owner?.Height ?? 917,
            CanResize = false,
            SystemDecorations = SystemDecorations.None,
            Background = Brushes.Transparent,
            TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent }
        };

        popupWindow.Closed += (s, e) =>
        {
            if (owner.Content is Control content)
                content.Effect = originalEffect;
        };

        await popupWindow.ShowDialog(owner);
    }
}
