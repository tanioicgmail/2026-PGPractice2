using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Media;
using KURAOrderSystem.Models;

namespace KURAOrderSystem.ViewModels
{
    public partial class MainViewModel
    {
        // Commands
        public ICommand SelectCategoryCommand { get; private set; } = null!;
        public ICommand SelectSushiCommand { get; private set; } = null!;
        public ICommand IncrementQuantityCommand { get; private set; } = null!;
        public ICommand DecrementQuantityCommand { get; private set; } = null!;
        public ICommand ConfirmOrderCommand { get; private set; } = null!;
        public ICommand CancelOrderConfirmCommand { get; private set; } = null!;
        public ICommand CallStaffCommand { get; private set; } = null!;
        public ICommand CloseStaffCallCommand { get; private set; } = null!;
        public ICommand ShowCheckoutCommand { get; private set; } = null!;
        public ICommand ConfirmCheckoutCommand { get; private set; } = null!;
        public ICommand CloseCheckoutCommand { get; private set; } = null!;
        public ICommand CloseDatabaseWarningCommand { get; private set; } = null!;

        private void InitializeCommands()
        {
            SelectCategoryCommand = new RelayCommand(p => SelectedCategory = p?.ToString() ?? "にぎり");
            SelectSushiCommand = new RelayCommand(p => ExecuteSelectSushi(p));
            IncrementQuantityCommand = new RelayCommand(_ => OrderQuantity++);
            DecrementQuantityCommand = new RelayCommand(_ => OrderQuantity--);
            ConfirmOrderCommand = new RelayCommand(_ => ExecuteConfirmOrder());
            CancelOrderConfirmCommand = new RelayCommand(_ => IsOrderConfirmVisible = false);
            CallStaffCommand = new RelayCommand(_ => ExecuteCallStaff());
            CloseStaffCallCommand = new RelayCommand(_ => IsStaffCallVisible = false);
            ShowCheckoutCommand = new RelayCommand(_ => ExecuteShowCheckout());
            ConfirmCheckoutCommand = new RelayCommand(_ => ExecuteConfirmCheckout());
            CloseCheckoutCommand = new RelayCommand(_ => IsCheckoutVisible = false);
            CloseDatabaseWarningCommand = new RelayCommand(_ => IsDatabaseWarningVisible = false);
        }

        private void ExecuteSelectSushi(object? parameter)
        {
            if (parameter is SushiItem item)
            {
                SelectedSushi = item;
                OrderQuantity = 1;
                IsOrderConfirmVisible = true;
            }
        }

        private void ExecuteCallStaff()
        {
            try { SystemSounds.Beep.Play(); } catch { }
            IsStaffCallVisible = true;
        }

        private void ExecuteShowCheckout()
        {
            try { SystemSounds.Asterisk.Play(); } catch { }
            IsCheckoutVisible = true;
        }

        private void ExecuteConfirmOrder()
        {
            if (SelectedSushi == null) return;

            // Play Order Placed Sound
            try { SystemSounds.Asterisk.Play(); } catch { }

            var newOrder = new OrderItem(SelectedSushi, OrderQuantity);
            OrderedHistory.Insert(0, newOrder); // Show newest first in UI

            GrandTotal += newOrder.TotalPrice;

            int oldPlateCount = PlateCount;
            if (SelectedSushi.IsPlateEligible)
            {
                PlateCount += OrderQuantity;
            }

            IsOrderConfirmVisible = false;

            // Save order to SQL Database (runs in background)
            var singleItemList = new List<OrderItem> { newOrder };
            if (IsSqlAvailable)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    _dbManager.SaveOrder(newOrder.TotalPrice, SelectedSushi.IsPlateEligible ? OrderQuantity : 0, singleItemList, out _);
                });
            }

            // Simulate kitchen preparing -> delivered flow
            SimulateKitchenDelivery(newOrder);

            // Bikkura Pon check (only if participating is enabled!)
            if (IsBikkuraPonEnabled)
            {
                int oldGamesCount = oldPlateCount / 5;
                int newGamesCount = PlateCount / 5;
                if (newGamesCount > oldGamesCount)
                {
                    int gamesToPlay = newGamesCount - oldGamesCount;
                    TriggerBikkuraPon(gamesToPlay);
                }
            }
        }

        private void ExecuteConfirmCheckout()
        {
            try { SystemSounds.Asterisk.Play(); } catch { }
            IsCheckoutVisible = false;
            IsCheckoutCompleteVisible = true;
        }
    }
}
