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
    private readonly int[] _boardSizes = { 7, 10 };
    private int _currentIndex = 0;

    [ObservableProperty]
    private string appName = "Sink Simulator";

    [ObservableProperty]
    private string authorName = "by xrepcim00";

    public int CurrentBoardSize => _boardSizes[_currentIndex];
    public string BoardSizeDisplay => $"{CurrentBoardSize}x{CurrentBoardSize}";

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
    private async Task Play(Window window)
    {
        try
        {
            await _apiService.UpdateSettingsAsync(BoardSizeDisplay);
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
            var popup = new MessagePopup { DataContext = viewModel };
            var popupWindow = new Window
            {
                Content = popup,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.WidthAndHeight,
                CanResize = false,
                Title = "Error"
            };
            await popupWindow.ShowDialog(window);
        }
    }
}
