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

public record ShipGroupData(int Count, Ship Representative);

public partial class PlanningBoardViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<Ship> _availableShips = new();

    [ObservableProperty]
    private ObservableCollection<Ship> _allShips = new();

    [ObservableProperty]
    private PlacedShip? _activeShip;

    public IReadOnlyDictionary<int, ShipGroupData> ShipGroups => AllShips 
        .GroupBy(s => s.Size)
        .OrderByDescending(g => g.Key)
        .ToDictionary(
            g => g.Key,
            g => new ShipGroupData(
                AvailableShips.Count(s => s.Size == g.Key),
                g.First()
            )
        );

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
        Task.Run(LoadPlanningDataAsync).GetAwaiter().GetResult();
    }

    private async Task LoadPlanningDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var planningData = await _apiService.GetPlanningDataAsync();

            PlayerGrid = planningData.PlayerGrid;

            AllShips.Clear();
            if (planningData.AllShips != null)
            {
                foreach (var ship in planningData.AllShips)
                {
                    AllShips.Add(ship);
                }
            }

            AvailableShips.Clear();
            if (planningData.AvailableShips != null)
            {
                foreach (var ship in planningData.AvailableShips)
                {
                    AvailableShips.Add(ship);
                }
            }

            ActiveShip = planningData.ActiveShip;

            GridIndices.Clear();
            for (int i = 0; i < PlayerGrid.GridSize * PlayerGrid.GridSize; i++)
            {
                GridIndices.Add(i);
            }

            AllShips.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShipGroups));
            AvailableShips.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShipGroups));
            OnPropertyChanged(nameof(ShipGroups));
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

    [RelayCommand]
    private async Task SelectShip(int size)
    {
        if (!ShipGroups.TryGetValue(size, out var group) || group.Count == 0) return;

        var shipToSelect = AvailableShips.FirstOrDefault(s => s.Size == size);
        if (shipToSelect == null) return;

        if (ActiveShip != null) 
            ActiveShip = null;

        ActiveShip = new PlacedShip
        {
            Id = shipToSelect.Id,
            Size = shipToSelect.Size,
            Color = shipToSelect.Color,
            Rotation = 0,
            Name = shipToSelect.Name,
            Row = -1,
            Col = -1
        };
    }

    [RelayCommand]
    private async Task PlaceOrGetShip(int index)
    {
        try
        {
            int gridSize = PlayerGrid.GridSize;
            int row = index / gridSize;
            int col = index % gridSize;

            await _apiService.SetActiveOrPlaceShipAsync(row, col, ActiveShip);
            await LoadPlanningDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Action failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ClearGrid()
    {
        try
        {
            await _apiService.ClearGridAsync();
            await LoadPlanningDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Action failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RotateActiveShip()
    {
        try
        {
            await _apiService.RotateActiveShipAsync();
            await LoadPlanningDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Action failed: {ex.Message}";
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
