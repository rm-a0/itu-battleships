using Avalonia.Controls;
using Avalonia.Interactivity;
using BattleshipsAvalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BattleshipsAvalonia.Views;

public partial class GameBoard : Window
{
    public GameBoard()
    {
        InitializeComponent();
        DataContext = Program.ServiceProvider.GetRequiredService<GameBoardViewModel>();
    }
}
