using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BattleshipsAvalonia.ViewModels;

public partial class ConfirmationPopupViewModel : ObservableObject
{
    [ObservableProperty]
    private string message = string.Empty;

    public bool IsConfirmed { get; private set; } = false;

    public ConfirmationPopupViewModel(string message)
    {
        Message = message;
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        IsConfirmed = false;
        window.Close();
    }

    [RelayCommand]
    private void Confirm(Window window)
    {
        IsConfirmed = true;
        window.Close();
    }
}
