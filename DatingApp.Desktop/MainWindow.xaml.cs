using System.Windows;
using DatingApp.Desktop.ViewModels;

namespace DatingApp.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}