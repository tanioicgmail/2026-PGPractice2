using System;
using System.Linq;
using System.Media;
using KURAOrderSystem.Models;

namespace KURAOrderSystem.ViewModels
{
    public partial class MainViewModel
    {
        // Event for triggering the Bikkura Pon window
        public event EventHandler<int>? TriggerBikkuraPonGame;

        private void InitializeDb()
        {
            bool success = _dbManager.InitializeDatabase();
            if (!success)
            {
                DatabaseWarningMessage = "SQL Server LocalDB への接続に失敗しました。\nアプリケーションは「オフライン・デモモード」で起動します。\n(注文はメモリ上に記録され、動作テストは可能です)";
                IsDatabaseWarningVisible = true;
            }
            else
            {
                // Retrieve historical orders from DB to populate OrderHistory (for persistent feel)
                var history = _dbManager.GetOrderHistory();
                int loadedPlates = 0;
                int totalAmount = 0;

                foreach (var item in history.AsEnumerable().Reverse())
                {
                    OrderedHistory.Add(item);
                    totalAmount += item.TotalPrice;
                    if (item.Item.IsPlateEligible)
                    {
                        loadedPlates += item.Quantity;
                    }
                }

                GrandTotal = totalAmount;
                PlateCount = loadedPlates;
            }
        }

        private void TriggerBikkuraPon(int gameCount)
        {
            TriggerBikkuraPonGame?.Invoke(this, gameCount);
        }

        public void SaveBikkuraPonResultToDb(int milestone, bool isWin, string prizeName)
        {
            if (IsSqlAvailable)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    _dbManager.SaveBikkuraPonResult(milestone, isWin, prizeName);
                });
            }
        }

        private void SimulateKitchenDelivery(OrderItem order)
        {
            // Simple timer simulation for status transitions
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5) // transition to preparing
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                if (order.Status == "注文済")
                {
                    order.Status = "調理中";

                    // start next timer to deliver
                    var deliveryTimer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(8)
                    };
                    deliveryTimer.Tick += (s2, e2) =>
                    {
                        deliveryTimer.Stop();
                        order.Status = "お届け済";

                        // Trigger Delivery Notification alert banner
                        ShowDeliveryNotification(order.Item.Name, order.Quantity);
                    };
                    deliveryTimer.Start();
                }
            };
            timer.Start();
        }

        private void ShowDeliveryNotification(string name, int qty)
        {
            ArrivingSushiName = name;
            ArrivingSushiQuantity = qty;
            IsDeliveryNotificationVisible = true;

            // Play arrival bell chime
            try { SystemSounds.Exclamation.Play(); } catch { }

            // Timer to automatically dismiss the banner after 4 seconds
            var dismissTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };
            dismissTimer.Tick += (s, e) =>
            {
                dismissTimer.Stop();
                IsDeliveryNotificationVisible = false;
            };
            dismissTimer.Start();
        }

        public void ResetSession()
        {
            // Clear current table orders (representing a new customer seating)
            OrderedHistory.Clear();
            GrandTotal = 0;
            PlateCount = 0;
            IsCheckoutCompleteVisible = false;
        }
    }
}
