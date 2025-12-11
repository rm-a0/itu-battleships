// Author: Michal Repcik (xrepcim00)
using Avalonia.Controls;
using Avalonia.Interactivity;
using BattleshipsAvalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BattleshipsAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = Program.ServiceProvider.GetRequiredService<MainWindowViewModel>();
    }
}
