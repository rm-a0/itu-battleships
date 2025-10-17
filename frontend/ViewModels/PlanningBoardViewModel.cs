using Avalonia.Controls;
using BattleshipsAvalonia.Models;
using BattleshipsAvalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BattleshipsAvalonia.ViewModels;

public partial class PlanningBoardViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<Ship> _availableShips = new();

    [ObservableProperty]
    private PlacedShip? _activeShip;

    [ObservableProperty]
    private Dictionary<string, string> _shipColors = new();
    
    [ObservableProperty]
    private Models.Grid _playerGrid = new();

    [ObservableProperty]
    private bool _isLoading = true;

    public PlanningBoardViewModel(IServiceProvider serviceProvider)
    {
        _apiService = serviceProvider.GetRequiredService<ApiService>();
        LoadPlanningDataAsync().ConfigureAwait(false);
    }

    private async Task LoadPlanningDataAsync()
    {
        try
        {
            IsLoading = true;

            AvailableShips.Clear();
            var availableShipsList = await _apiService.GetAvailableShipsAsync();
            foreach (var ship in availableShipsList)
            {
                AvailableShips.Add(ship);
            }

            ActiveShip = await _apiService.GetActiveShipAsync();
            ShipColors = await _apiService.GetShipColorsAsync();
            PlayerGrid = await _apiService.GetPlayerGridAsync();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading planning data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
