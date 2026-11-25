using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FinDesk.Views;

public partial class ConfirmationView : Window
{
    public bool Result { get; private set; }

    public ConfirmationView(string message)
    {
        InitializeComponent();
        MessageText.Text = message;
    }

    private void Yes_Click(object? sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void No_Click(object? sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }
}