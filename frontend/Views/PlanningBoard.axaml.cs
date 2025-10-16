using Avalonia.Controls;
using Avalonia.Interactivity;
using BattleshipsAvalonia.ViewModels;

namespace BattleshipsAvalonia.Views;

public partial class PlanningBoard : Window
{
    public PlanningBoard()
    {
        InitializeComponent();
        DataContext = new PlanningBoardViewModel();
    }
}
