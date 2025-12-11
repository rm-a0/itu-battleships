using Avalonia.Controls;
using BattleshipsAvalonia.Models;
using BattleshipsAvalonia.Services;
using BattleshipsAvalonia.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BattleshipsAvalonia.ViewModels;

public partial class GameBoardViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly IServiceProvider _serviceProvider;
    private Window? _parentWindow;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private Models.Grid _playerGrid = new();

    [ObservableProperty]
    private Models.Grid _pcGrid = new();

    [ObservableProperty]
    private ObservableCollection<int> _gridIndices = new();

    private string _difficulty = "easy";

    public GameBoardViewModel(IServiceProvider serviceProvider, ApiService apiService)
    {
        _serviceProvider = serviceProvider;
        _apiService = apiService;
        Task.Run(LoadGameDataAsync).GetAwaiter().GetResult();
    }

    public void SetParentWindow(Window window)
    {
        _parentWindow = window ?? throw new ArgumentNullException(nameof(window));
    }

    private async Task LoadGameDataAsync()
    {
        try
        {
            IsLoading = true;

            PlayerGrid = await _apiService.GetPlayerGridAsync();
            PcGrid = await _apiService.GetPcGridAsync();

            // Cache difficulty setting at game start
            var settings = await _apiService.GetSettingsAsync();
            _difficulty = settings.Difficulty ?? "easy";

            GridIndices.Clear();
            for (int i = 0; i < PlayerGrid.GridSize * PlayerGrid.GridSize; i++)
            {
                GridIndices.Add(i);
            }
        }
        catch (Exception ex)
        {
            await ShowPopupAsync($"Error loading game data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void CloseWindow()
    {
        _parentWindow?.Close();
    }

    [RelayCommand]
    private async Task Surrender()
    {
        await ShowPopupAsync("Do you really want to surrender?");
        await ShowPopupAsync("You Lost!");
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        _parentWindow?.Close();
    }

    [RelayCommand]
    private async Task SurrenderWithConfirm()
    {
        var confirmViewModel = new ConfirmationPopupViewModel("Do you really want to surrender?");
        await MessagePopupService.ShowPopupAsync<ConfirmationPopup, ConfirmationPopupViewModel>(_parentWindow!, confirmViewModel);
        
        if (confirmViewModel.IsConfirmed)
        {
            await ShowPopupAsync("You Lost!");
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            _parentWindow?.Close();
        }
    }

    [RelayCommand]
    private async Task ShootShip(int index)
    {
        try
        {
            IsLoading = true;

            if (!await ShootPcTile(index))
                return;
            await ShootRandomPlayerTile();
            await CheckGameEnd();
        }
        catch (Exception ex)
        {
            await ShowPopupAsync($"Error processing shot: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<bool> ShootPcTile(int index)
    {
        int row = index / PcGrid.GridSize;
        int col = index % PcGrid.GridSize;

        if (row < 0 || row >= PcGrid.GridSize || col < 0 || col >= PcGrid.GridSize)
            return false;

        string currentTile = PcGrid.Tiles[row][col];
        if (currentTile == "hit" || currentTile == "miss")
            return false;

        PcGrid.Tiles[row][col] = currentTile.StartsWith("ship-") ? "hit" : "miss";
        await _apiService.UpdatePcGridAsync(PcGrid.GridSize, PcGrid.Tiles);

        PcGrid = await _apiService.GetPcGridAsync();
        OnPropertyChanged(nameof(PcGrid));

        return true;
    }

    private async Task ShootRandomPlayerTile()
    {
        try
        {
            // Use cached difficulty setting
            var aiShot = await _apiService.GetAiShotAsync(PlayerGrid.GridSize, PlayerGrid.Tiles, _difficulty);

            // Update the player grid with the shot result
            PlayerGrid.Tiles[aiShot.Row][aiShot.Col] = aiShot.Result;

            await _apiService.UpdatePlayerGridAsync(PlayerGrid, mode: null);

            PlayerGrid = await _apiService.GetPlayerGridAsync();
            OnPropertyChanged(nameof(PlayerGrid));
        }
        catch (Exception ex)
        {
            await ShowPopupAsync($"Error processing PC shot: {ex.Message}");
        }
    }

    private async Task CheckGameEnd()
    {
        bool hasWon = _apiService.CheckForWin(PcGrid.Tiles);
        bool hasLost = _apiService.CheckForWin(PlayerGrid.Tiles);
        if (hasWon || hasLost)
        {
            await _apiService.SetCurrentScreenAsync("win");
            string message = hasWon ? "You won!" : "You lost!";
            await ShowPopupAsync(message);
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            _parentWindow?.Close();
        }
    }

    private async Task ShowPopupAsync(string message)
    {
        if (_parentWindow == null)
        {
            System.Diagnostics.Debug.WriteLine($"Popup not shown: {message} (Parent window not set)");
            return;
        }
        var viewModel = new MessagePopupViewModel(message);
        await MessagePopupService.ShowPopupAsync<MessagePopup, MessagePopupViewModel>(_parentWindow, viewModel);
    }
}
