using Avalonia.Controls;
using BattleshipsAvalonia.Models;
using BattleshipsAvalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BattleshipsAvalonia.ViewModels;

public record CellPosition(int Row, int Col);

public record ShipType(string Name, int Size, string Color, int Count);

public partial class PlanningBoardViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<Ship> _availableShips = new();

    [ObservableProperty]
    private ObservableCollection<ShipType> _availableShipTypes = new();

    [ObservableProperty]
    private PlacedShip? _activeShip;

    [ObservableProperty]
    private Models.Grid _playerGrid = new();

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private ObservableCollection<int> _gridIndices = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public PlanningBoardViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _apiService = serviceProvider.GetRequiredService<ApiService>();
        LoadPlanningDataAsync().ConfigureAwait(false);
    }

    private async Task LoadPlanningDataAsync()
    {
        try
        {
            IsLoading = true;

            var availableShipsList = await _apiService.GetAvailableShipsAsync();
            AvailableShips = new ObservableCollection<Ship>(availableShipsList);

            AvailableShipTypes = new ObservableCollection<ShipType>(
                availableShipsList
                    .GroupBy(s => s.Name)
                    .Select(g => new ShipType(
                        Name: g.Key,
                        Size: g.First().Size,
                        Color: g.First().Color,
                        Count: g.Count()
                    ))
            );

            ActiveShip = await _apiService.GetActiveShipAsync();
            PlayerGrid = await _apiService.GetPlayerGridAsync();

            GridIndices.Clear();
            for (int i = 0; i < PlayerGrid.GridSize * PlayerGrid.GridSize; i++)
            {
                GridIndices.Add(i);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading planning data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanPlaceShip(CellPosition pos, PlacedShip ship, Models.Grid grid)
    {
        int endRow = ship.Rotation == 0 ? pos.Row : pos.Row + ship.Size - 1;
        int endCol = ship.Rotation == 0 ? pos.Col + ship.Size - 1 : pos.Col;
        if (endRow >= grid.GridSize || endCol >= grid.GridSize)
        {
            System.Diagnostics.Debug.WriteLine($"Cannot place ship: Out of bounds (Row={pos.Row}, Col={pos.Col}, Size={ship.Size}, Rotation={ship.Rotation})");
            return false;
        }
        for (int i = pos.Row; i <= endRow; i++)
        for (int j = pos.Col; j <= endCol; j++)
        {
            if (i < 0 || j < 0 || grid.Tiles[i][j] != "empty")
            {
                System.Diagnostics.Debug.WriteLine($"Cannot place ship: Invalid cell (Row={i}, Col={j}, Tile={grid.Tiles[i][j]})");
                return false;
            }
        }
        return true;
    }

    [RelayCommand]
    private void SelectShip(ShipType shipType)
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;

            // Find the first available ship of this type
            var ship = AvailableShips.FirstOrDefault(s => s.Name == shipType.Name);
            if (ship == null)
            {
                ErrorMessage = $"No {shipType.Name} available.";
                return;
            }

            ActiveShip = new PlacedShip
            {
                Id = ship.Id,
                Size = ship.Size,
                Color = ship.Color,
                Rotation = ship.Rotation,
                Name = ship.Name,
                Row = -1,
                Col = -1
            };
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error selecting ship: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PlaceShip(CellPosition position)
    {
        if (ActiveShip == null || IsLoading) return;
        try
        {
            IsLoading = true;
            if (!CanPlaceShip(position, ActiveShip, PlayerGrid))
            {
                ErrorMessage = "Cannot place ship at this position.";
                return;
            }
            var placedShip = new PlacedShip
            {
                Id = ActiveShip.Id,
                Size = ActiveShip.Size,
                Color = ActiveShip.Color,
                Rotation = ActiveShip.Rotation,
                Name = ActiveShip.Name,
                Row = position.Row,
                Col = position.Col
            };

            await _apiService.AddPlacedShipAsync(placedShip);
            await _apiService.RemoveActiveShipAsync();
            await LoadPlanningDataAsync(); // Reloads AvailableShipTypes
            AvailableShips.Remove(AvailableShips.First(s => s.Id == ActiveShip.Id));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error placing ship: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RotateShip()
    {
        if (ActiveShip == null || IsLoading) return;

        try
        {
            IsLoading = true;
            await _apiService.RotateActiveShipAsync();
            await LoadPlanningDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error rotating ship: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ClearGrid()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            await _apiService.ClearGridAsync();
            await _apiService.RestoreAvailableShipsAsync();
            await LoadPlanningDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error clearing grid: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartGame(Window window)
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            await _apiService.SetCurrentScreenAsync("game");
            //var gameWindow = _serviceProvider.GetRequiredService<GameBoard>();
            //gameWindow.Show();
            window.Close();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error starting game: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
