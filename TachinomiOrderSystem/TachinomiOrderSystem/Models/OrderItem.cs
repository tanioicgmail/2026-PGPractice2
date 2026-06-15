using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TachinomiOrderSystem.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        private int _quantity;
        private DateTime _orderTime;

        public MenuItem Item { get; }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPrice));
                    OnPropertyChanged(nameof(FormattedTotalPrice));
                }
            }
        }

        public DateTime OrderTime
        {
            get => _orderTime;
            set
            {
                if (_orderTime != value)
                {
                    _orderTime = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FormattedOrderTime));
                }
            }
        }

        public int TotalPrice => Item.Price * Quantity;
        public string FormattedTotalPrice => $"{TotalPrice}円";
        public string FormattedOrderTime => OrderTime.ToString("HH:mm:ss");

        public OrderItem(MenuItem item, int quantity)
        {
            Item = item;
            Quantity = quantity;
            OrderTime = DateTime.Now;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
