using Avalonia.Controls;
using FinDesk.ViewModels;

namespace FinDesk.Views;

public partial class CategoryManagerView : Window
{
    private CategoryManagerViewModel _viewModel;

    public CategoryManagerView()
    {
        InitializeComponent();
        _viewModel = new CategoryManagerViewModel();
        DataContext = _viewModel;

        _viewModel.Closed += (saved) =>
        {
            this.Close();
        };
    }
}