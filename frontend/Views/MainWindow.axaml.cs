using Avalonia.Controls;
using Avalonia.Interactivity;
using BattleshipsAvalonia.ViewModels;

namespace BattleshipsAvalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
