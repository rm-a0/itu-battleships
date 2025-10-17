using Avalonia.Controls;
using Avalonia.Interactivity;
using BattleshipsAvalonia.ViewModels;
using BattleshipsAvalonia.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BattleshipsAvalonia.Views;

public partial class PlanningBoard : Window
{
    public PlanningBoard()
    {
        InitializeComponent();
        DataContext = Program.ServiceProvider.GetRequiredService<PlanningBoardViewModel>();
    }
}
