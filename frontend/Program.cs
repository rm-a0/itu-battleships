using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using BattleshipsAvalonia.Services;
using BattleshipsAvalonia.ViewModels;
using BattleshipsAvalonia.Views;

namespace BattleshipsAvalonia;

class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public static void Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddSingleton<ApiService>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<PlanningBoardViewModel>();
        services.AddTransient<GameBoardViewModel>();
        services.AddTransient<MessagePopupViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<PlanningBoard>();
        services.AddTransient<GameBoard>();
        services.AddTransient<MessagePopup>();

        ServiceProvider = services.BuildServiceProvider();

        BuildAvaloniaApp()
            .WithInterFont()
            .UsePlatformDetect()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
