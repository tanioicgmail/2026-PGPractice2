using System.Windows;
using TachinomiOrderSystem.ViewModels;

namespace TachinomiOrderSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
