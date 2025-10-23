using Avalonia.Controls;
using BattleshipsAvalonia.Models;
using BattleshipsAvalonia.Services;
using BattleshipsAvalonia.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BattleshipsAvalonia.ViewModels;

public partial class GameBoardViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private Models.Grid _playerGrid = new();

    [ObservableProperty]
    private Models.Grid _pcGrid = new();

    [ObservableProperty]
    private ObservableCollection<int> _gridIndices = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public GameBoardViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _apiService = serviceProvider.GetRequiredService<ApiService>();
        Task.Run(LoadGameDataAsync).GetAwaiter().GetResult();
    }

    private async Task LoadGameDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            PlayerGrid = await _apiService.GetPlayerGridAsync();
            PcGrid = await _apiService.GetPcGridAsync();

            GridIndices.Clear();
            for (int i = 0; i < PlayerGrid.GridSize * PlayerGrid.GridSize; i++)
            {
                GridIndices.Add(i);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading game data: {ex.Message}";

        }
    }

    [RelayCommand]
    private async Task ShootShip(int index)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            if (!await ShootPcTile(index))
                return;
            await ShootRandomPlayerTile();
            await CheckGameEnd();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error processing shot: {ex.Message}";
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
            var validCells = new System.Collections.Generic.List<(int row, int col)>();
            for (int r = 0; r < PlayerGrid.GridSize; r++)
            {
                for (int c = 0; c < PlayerGrid.GridSize; c++)
                {
                    string tile = PlayerGrid.Tiles[r][c];
                    if (tile != "hit" && tile != "miss")
                        validCells.Add((r, c));
                }
            }

            if (validCells.Count == 0)
                return;

            var random = new Random();
            var (row, col) = validCells[random.Next(validCells.Count)];

            string currentTile = PlayerGrid.Tiles[row][col];
            PlayerGrid.Tiles[row][col] = currentTile == "empty" ? "miss" : "hit";

            await _apiService.UpdatePlayerGridAsync(PlayerGrid, mode: null);

            PlayerGrid = await _apiService.GetPlayerGridAsync();
            OnPropertyChanged(nameof(PlayerGrid));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error processing PC shot: {ex.Message}";
        }
    }

    private async Task CheckGameEnd()
    {
        bool hasWon = _apiService.CheckForWin(PcGrid.Tiles);
        bool hasLost = _apiService.CheckForWin(PlayerGrid.Tiles);
        if (hasWon || hasLost)
        {
            await _apiService.SetCurrentScreenAsync("win");
            if (hasLost)
                Console.WriteLine("you Lost");
            if (hasWon)
                Console.WriteLine("you won");
            //change window
        }
    }
}
