using System.Collections.Generic;
using KURAOrderSystem.Models;

namespace KURAOrderSystem.Services
{
    public class MenuService : IMenuService
    {
        public List<SushiItem> GetMenu()
        {
            return new List<SushiItem>
            {
                // にぎり (Nigiri Sushi)
                new SushiItem { Id = 1, Name = "まぐろ", NameEn = "Tuna", Price = 115, Category = "にぎり", Tag = "人気", IsPlateEligible = true },
                new SushiItem { Id = 2, Name = "極み熟成 真鯛", NameEn = "Sea Bream", Price = 230, Category = "にぎり", Tag = "贅沢", IsPlateEligible = true },
                new SushiItem { Id = 3, Name = "サーモン", NameEn = "Salmon", Price = 115, Category = "にぎり", Tag = "人気", IsPlateEligible = true },
                new SushiItem { Id = 4, Name = "炙りチーズサーモン", NameEn = "Seared Cheese Salmon", Price = 165, Category = "にぎり", Tag = "おすすめ", IsPlateEligible = true },
                new SushiItem { Id = 5, Name = "えび天にぎり", NameEn = "Shrimp Tempura Sushi", Price = 115, Category = "にぎり", Tag = "揚げたて", IsPlateEligible = true },
                new SushiItem { Id = 6, Name = "たまご焼き", NameEn = "Egg Omelet", Price = 115, Category = "にぎり", Tag = "定番", IsPlateEligible = true },
                new SushiItem { Id = 7, Name = "つぶ貝", NameEn = "Whelk", Price = 165, Category = "にぎり", Tag = "定番", IsPlateEligible = true },
                new SushiItem { Id = 8, Name = "本まぐろ 中とろ", NameEn = "Medium Fatty Tuna", Price = 230, Category = "にぎり", Tag = "一推し", IsPlateEligible = true },

                // 軍艦・細巻 (Gunkan/Maki Roll)
                new SushiItem { Id = 9, Name = "ねぎまぐろ軍艦", NameEn = "Minced Tuna", Price = 115, Category = "軍艦・細巻", Tag = "人気", IsPlateEligible = true },
                new SushiItem { Id = 10, Name = "サラダ軍艦", NameEn = "Salad Gunkan", Price = 115, Category = "軍艦・細巻", Tag = "お子様に人気", IsPlateEligible = true },
                new SushiItem { Id = 11, Name = "コーン軍艦", NameEn = "Sweet Corn Mayo", Price = 115, Category = "軍艦・細巻", Tag = "定番", IsPlateEligible = true },
                new SushiItem { Id = 12, Name = "いくら軍艦", NameEn = "Salmon Roe", Price = 230, Category = "軍艦・細巻", Tag = "贅沢", IsPlateEligible = true },
                new SushiItem { Id = 13, Name = "鉄火巻", NameEn = "Tuna Roll", Price = 165, Category = "軍艦・細巻", Tag = "定番", IsPlateEligible = true },
                new SushiItem { Id = 14, Name = "かっぱ巻", NameEn = "Cucumber Roll", Price = 115, Category = "軍艦・細巻", Tag = "定番", IsPlateEligible = true },

                // サイドメニュー (Sides)
                new SushiItem { Id = 15, Name = "特製茶碗蒸し", NameEn = "Chawanmushi (Steamed Egg)", Price = 230, Category = "サイドメニュー", Tag = "手作り", IsPlateEligible = false },
                new SushiItem { Id = 16, Name = "醤油らーめん", NameEn = "Shoyu Ramen", Price = 450, Category = "サイドメニュー", Tag = "こだわり出汁", IsPlateEligible = false },
                new SushiItem { Id = 17, Name = "旨だれからあげ", NameEn = "Fried Chicken", Price = 360, Category = "サイドメニュー", Tag = "揚げたて", IsPlateEligible = false },
                new SushiItem { Id = 18, Name = "フライドポテト", NameEn = "French Fries", Price = 280, Category = "サイドメニュー", Tag = "大人気", IsPlateEligible = false },
                new SushiItem { Id = 19, Name = "味噌汁", NameEn = "Miso Soup", Price = 180, Category = "サイドメニュー", Tag = "定番", IsPlateEligible = false },

                // デザート・ドリンク (Desserts/Drinks)
                new SushiItem { Id = 20, Name = "ぷるぷるプリン", NameEn = "Custard Pudding", Price = 250, Category = "デザート・ドリンク", Tag = "自家製", IsPlateEligible = false },
                new SushiItem { Id = 21, Name = "ミルククレープ", NameEn = "Mille Crepe", Price = 280, Category = "デザート・ドリンク", Tag = "おすすめ", IsPlateEligible = false },
                new SushiItem { Id = 22, Name = "バニラアイス", NameEn = "Vanilla Ice Cream", Price = 150, Category = "デザート・ドリンク", Tag = "定番", IsPlateEligible = false },
                new SushiItem { Id = 23, Name = "生ビール", NameEn = "Draft Beer", Price = 600, Category = "デザート・ドリンク", Tag = "冷えてます", IsPlateEligible = false },
                new SushiItem { Id = 24, Name = "オレンジジュース", NameEn = "Orange Juice", Price = 150, Category = "デザート・ドリンク", Tag = "果汁100%", IsPlateEligible = false }
            };
        }
    }
}
