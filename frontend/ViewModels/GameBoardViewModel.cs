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

    public GameBoardViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _apiService = serviceProvider.GetRequiredService<ApiService>();
    }
}
