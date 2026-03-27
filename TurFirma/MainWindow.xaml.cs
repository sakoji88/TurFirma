using System.Windows;
using TurFirma.ViewModels;

namespace TurFirma;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
