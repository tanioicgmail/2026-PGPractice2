using System.Collections.Generic;
using TachinomiOrderSystem.Models;

namespace TachinomiOrderSystem.Services
{
    public class MenuService
    {
        public List<MenuItem> GetMenu()
        {
            return new List<MenuItem>
            {
                new MenuItem { Id = 1, Name = "生ビール", Price = 500 },
                new MenuItem { Id = 2, Name = "枝豆", Price = 300 }
            };
        }
    }
}
