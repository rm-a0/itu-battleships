using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BattleshipsAvalonia.Views;

public partial class MainWindow : Window
{
    private string[] _boardSizes = { "8x8", "10x10", "12x12" };
    private int _currentIndex = 0;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnPlayButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void OnDecreaseBoardSize(object? sender, RoutedEventArgs e)
        {
            _currentIndex = (_currentIndex - 1 + _boardSizes.Length) % _boardSizes.Length;
            BoardSizeDisplay.Text = _boardSizes[_currentIndex];
            // Update your settings or backend call here, e.g., based on the selected size
        }

        private void OnIncreaseBoardSize(object? sender, RoutedEventArgs e)
        {
            _currentIndex = (_currentIndex + 1) % _boardSizes.Length;
            BoardSizeDisplay.Text = _boardSizes[_currentIndex];
            // Update your settings or backend call here, e.g., based on the selected size
        }
}
