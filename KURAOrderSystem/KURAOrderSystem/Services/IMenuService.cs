using System.Collections.Generic;
using KURAOrderSystem.Models;

namespace KURAOrderSystem.Services
{
    public interface IMenuService
    {
        List<SushiItem> GetMenu();
    }
}
