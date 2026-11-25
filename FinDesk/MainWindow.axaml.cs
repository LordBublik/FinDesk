using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FinDesk.Models;
using FinDesk.ViewModels;
using FinDesk.Views;

namespace FinDesk;

public partial class MainWindow : Window
{
    private MainViewModel _mainViewModel;

    public MainWindow()
    {
        InitializeComponent();
        _mainViewModel = new MainViewModel(this);
        this.DataContext = _mainViewModel;

        var currentTheme = ThemeManager.GetCurrentTheme();
        BtnTheme.Content = currentTheme == "dark" ? "☀️" : "🌙";
    }

    private void ShowTransactions_Click(object? sender, RoutedEventArgs e)
    {
        TransactionsPanel.IsVisible = true;
        StatisticsPanel.IsVisible = false;

        BtnTransactions.Background = Brushes.White;
        BtnTransactions.Foreground = new SolidColorBrush(Color.Parse("#2196F3"));

        BtnStatistics.Background = Brushes.Transparent;
        BtnStatistics.Foreground = Brushes.White;
    }

    private void ShowStatistics_Click(object? sender, RoutedEventArgs e)
    {
        TransactionsPanel.IsVisible = false;
        StatisticsPanel.IsVisible = true;

        StatisticsPanel.RefreshData();

        BtnStatistics.Background = Brushes.White;
        BtnStatistics.Foreground = new SolidColorBrush(Color.Parse("#2196F3"));

        BtnTransactions.Background = Brushes.Transparent;
        BtnTransactions.Foreground = Brushes.White;
    }

    private async void ShowCategories_Click(object? sender, RoutedEventArgs e)
    {
        var categoryManager = new CategoryManagerView();
        await categoryManager.ShowDialog(this);
        _mainViewModel.LoadData();
    }

    private void ToggleTheme_Click(object? sender, RoutedEventArgs e)
    {
        ThemeManager.ToggleTheme();

        var currentTheme = ThemeManager.GetCurrentTheme();
        BtnTheme.Content = currentTheme == "dark" ? "☀️" : "🌙";

        if (StatisticsPanel.IsVisible)
        {
            StatisticsPanel.RefreshData();
        }
    }
}