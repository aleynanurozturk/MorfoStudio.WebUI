using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MorfoStudio.WebUI.Models;
using MorfoStudio.WebUI.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MorfoStudio.WebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IShopierScraper _scraper;
        private readonly IMemoryCache _cache;

        private const string ShopUrl = "https://www.shopier.com/studiomorfo";
        private const string CacheKey = "home_products";

        public HomeController(IShopierScraper scraper, IMemoryCache cache)
        {
            _scraper = scraper;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            // Tamamen dinamik: yalnızca Shopier verisi
            if (!_cache.TryGetValue(CacheKey, out SlideshowViewModel vm))
            {
                var products = await _scraper.GetProductsAsync(ShopUrl);
                vm = new SlideshowViewModel
                {
                    ImageUrls = products
                        .Select(p => p.ImageUrl)
                        .Where(u => !string.IsNullOrWhiteSpace(u))
                        .Distinct()
                        .ToList()
                };

                _cache.Set(CacheKey, vm, TimeSpan.FromMinutes(10)); // istersen kaldır
            }

            return View(vm); // boş gelirse view boş state gösterecek
        }

        public IActionResult About() => View();
        public IActionResult Contact() => View();
        public IActionResult Privacy() => View();
    }
}
