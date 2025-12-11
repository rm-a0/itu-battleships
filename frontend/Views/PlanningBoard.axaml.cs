// Author: Michal Repcik (xrepcim00)
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BattleshipsAvalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace BattleshipsAvalonia.Views;

public partial class PlanningBoard : Window
{
    public PlanningBoard()
    {
        InitializeComponent();
        var viewModel = Program.ServiceProvider.GetRequiredService<PlanningBoardViewModel>();
        viewModel.SetParentWindow(this);
        DataContext = viewModel;
    }
}
