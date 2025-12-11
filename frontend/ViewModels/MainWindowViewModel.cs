// Author: Michal Repcik (xrepcim00)
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using BattleshipsAvalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using BattleshipsAvalonia.Services;

namespace BattleshipsAvalonia.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ApiService _apiService;
    private readonly int[] _boardSizes = { 6, 7, 8, 9 };
    private readonly string[] _difficulties = { "Easy", "Medium", "Hard" };
    private int _currentIndex = 0;
    private int _difficultyIndex = 0;

    [ObservableProperty]
    private string appName = "Sink Simulator";

    [ObservableProperty]
    private string authorName = "by xrepcim00";

    public int CurrentBoardSize => _boardSizes[_currentIndex];
    public string BoardSizeDisplay => $"{CurrentBoardSize}x{CurrentBoardSize}";
    public string CurrentDifficulty => _difficulties[_difficultyIndex];

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _apiService = serviceProvider.GetRequiredService<ApiService>();
    }

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
    private void DecreaseDifficulty()
    {
        _difficultyIndex = (_difficultyIndex - 1 + _difficulties.Length) % _difficulties.Length;
        OnPropertyChanged(nameof(CurrentDifficulty));
    }

    [RelayCommand]
    private void IncreaseDifficulty()
    {
        _difficultyIndex = (_difficultyIndex + 1) % _difficulties.Length;
        OnPropertyChanged(nameof(CurrentDifficulty));
    }

    [RelayCommand]
    private async Task Play(Window window)
    {
        try
        {
            await _apiService.UpdateSettingsAsync(BoardSizeDisplay, CurrentDifficulty.ToLower());
            await _apiService.CreateGridsAsync(CurrentBoardSize);
            await _apiService.SetAvailableShipsAsync();
            await _apiService.SetCurrentScreenAsync("planning");
            var planningWindow = _serviceProvider.GetRequiredService<PlanningBoard>();
            planningWindow.Show();
            window.Close();
        }
        catch (Exception ex)
        {
            var viewModel = new MessagePopupViewModel(ex.Message);
            await MessagePopupService.ShowPopupAsync<MessagePopup, MessagePopupViewModel>(window, viewModel);
        }
    }
}
