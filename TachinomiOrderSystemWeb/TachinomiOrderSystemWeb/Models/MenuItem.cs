namespace TOSWeb.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public string FormattedPrice => $"{Price}円 (税込)";
    }
}
