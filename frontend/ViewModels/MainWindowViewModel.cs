using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using BattleshipsAvalonia.Views;

namespace BattleshipsAvalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly int[] _boardSizes = { 7, 10 };
    private int _currentIndex = 0;

    [ObservableProperty]
    private string appName = "Sink Simulator";

    [ObservableProperty]
    private string authorName = "by xrepcim00";

    public int CurrentBoardSize => _boardSizes[_currentIndex];
    public string BoardSizeDisplay => $"{CurrentBoardSize}x{CurrentBoardSize}";

    [RelayCommand]
    private void DecreaseBoardSize()
    {
        _currentIndex = (_currentIndex - 1 + _boardSizes.Length) % _boardSizes.Length;
        OnPropertyChanged(nameof(BoardSizeDisplay));
    }

    [RelayCommand]
    private void IncreaseBoardSize()
    {
        _currentIndex = (_currentIndex + 1) % _boardSizes.Length;
        OnPropertyChanged(nameof(BoardSizeDisplay));
    }

    [RelayCommand]
    private void Play(Window window)
    {
        var planningWindow = new PlanningBoard();
        planningWindow.Show();

        window.Close();
    }
}
