namespace KURAOrderSystem.Models
{
    public class SushiItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public int Price { get; set; }
        public string Category { get; set; } = string.Empty; // にぎり, 軍艦, サイド, デザート
        public string ImagePath { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty; // e.g. "おすすめ", "新商品", "定番"
        public bool IsPlateEligible { get; set; } = true; // Does this count towards Bikkura Pon? (Usually yes for sushi plates, no for drinks/ramen)
        
        public string FormattedPrice => $"{Price}円 (税込)";
    }
}
