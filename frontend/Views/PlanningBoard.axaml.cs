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
        DataContext = Program.ServiceProvider.GetRequiredService<PlanningBoardViewModel>();
    }
}
