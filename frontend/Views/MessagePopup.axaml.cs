// Author: Michal Repcik (xrepcim00)
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BattleshipsAvalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace BattleshipsAvalonia.Views;

public partial class MessagePopup : UserControl
{
    public MessagePopup()
    {
        InitializeComponent();
    }
}
