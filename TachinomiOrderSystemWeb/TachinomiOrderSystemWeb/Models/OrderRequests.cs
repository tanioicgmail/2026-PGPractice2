using System.Collections.Generic;

namespace TOSWeb.Models
{
    public class CheckoutRequest
    {
        public int TotalAmount { get; set; }
        public List<CheckoutItemRequest> Items { get; set; } = new();
    }

    public class CheckoutItemRequest
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Price { get; set; }
        public int Quantity { get; set; }
    }
}
