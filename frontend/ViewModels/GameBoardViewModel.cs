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
        Console.WriteLine("shoot ship");
    }
}
