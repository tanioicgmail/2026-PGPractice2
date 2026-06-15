using System;
using System.Windows;
using KURAOrderSystem.ViewModels;
using KURAOrderSystem.Views;

namespace KURAOrderSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.TriggerBikkuraPonGame += ViewModel_TriggerBikkuraPonGame;
            }
        }

        private void ViewModel_TriggerBikkuraPonGame(object? sender, int gameCount)
        {
            if (DataContext is MainViewModel viewModel)
            {
                // Play games sequentially
                for (int i = 0; i < gameCount; i++)
                {
                    // Calculate plate milestone (e.g. 5, 10, 15, etc.)
                    int currentMilestone = ((viewModel.PlateCount - (gameCount - 1 - i) * 5) / 5) * 5;
                    if (currentMilestone <= 0) currentMilestone = 5;

                    var bikkuraPonWin = new BikkuraPonWindow(viewModel, currentMilestone);
                    bikkuraPonWin.ShowDialog();
                }
            }
        }

        private void ClearSession_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.ResetSession();
            }
        }
    }
}