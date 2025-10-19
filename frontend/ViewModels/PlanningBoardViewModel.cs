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
    private PlacedShip? _activeShip;

    public IReadOnlyDictionary<int, ShipGroupData> ShipGroups => AvailableShips
        .GroupBy(s => s.Size)
        .OrderByDescending(g => g.Key)
        .ToDictionary(
            g => g.Key,
            g => new ShipGroupData(g.Count(), g.First())
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

            await _apiService.SetAvailableShipsAsync();
            PlayerGrid = await _apiService.GetPlayerGridAsync();
            var ships = await _apiService.GetAvailableShipsAsync();

            AvailableShips.Clear();
            foreach (var ship in ships)
            {
                AvailableShips.Add(ship);
            }

            ActiveShip = await _apiService.GetActiveShipAsync();

            GridIndices.Clear();
            for (int i = 0; i < PlayerGrid.GridSize * PlayerGrid.GridSize; i++)
            {
                GridIndices.Add(i);
            }

            AvailableShips.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(ShipGroups));
            };
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
    private void SelectShip(int size)
    {
        if (ActiveShip != null) return;

        if (!ShipGroups.TryGetValue(size, out var group) || group.Count == 0) return;

        var shipToSelect = group.Representative;

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
    private void PlaceShip()
    {
        if (ActiveShip == null) return;
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
