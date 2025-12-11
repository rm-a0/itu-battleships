// Author: Michal Repcik (xrepcim00)
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using BattleshipsAvalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using BattleshipsAvalonia.Services;

namespace BattleshipsAvalonia.ViewModels;

public partial class MessagePopupViewModel : ObservableObject
{
    [ObservableProperty]
    private string message = string.Empty;

    public MessagePopupViewModel(string message)
    {
        Message = message;
    }

    [RelayCommand]
    private void Close(Window window)
    {
        window.Close();
    }
    
}
