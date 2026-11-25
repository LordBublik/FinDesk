using Avalonia.Controls;
using FinDesk.ViewModels;

namespace FinDesk.Views;

public partial class StatisticsView : UserControl
{
    private StatisticsViewModel _viewModel;

    public StatisticsView()
    {
        InitializeComponent();
        _viewModel = new StatisticsViewModel();
        DataContext = _viewModel;
    }

    public void RefreshData()
    {
        _viewModel.Refresh();
    }
}