using System;

namespace TOSWeb.Models
{
    public class OrderItem
    {
        public MenuItem Item { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime OrderTime { get; set; }

        public int TotalPrice => Item.Price * Quantity;
        public string FormattedTotalPrice => $"{TotalPrice}円";
        public string FormattedOrderTime => OrderTime.ToString("HH:mm:ss");

        public OrderItem() { }

        public OrderItem(MenuItem item, int quantity)
        {
            Item = item;
            Quantity = quantity;
            OrderTime = DateTime.Now;
        }
    }
}
