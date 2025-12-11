// Author: Michal Repcik (xrepcim00)
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
        var viewModel = Program.ServiceProvider.GetRequiredService<GameBoardViewModel>();
        viewModel.SetParentWindow(this);
        DataContext = viewModel;
    }
}
