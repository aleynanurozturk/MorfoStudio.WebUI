using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MorfoStudio.WebUI.Models;
using MorfoStudio.WebUI.Services;
using System.Collections.Generic;

namespace MorfoStudio.WebUI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IShopierScraper _scraper;
        private readonly IMemoryCache _cache;

        // Shop URL’in: vitrin/listing sayfan (ör: https://www.shopier.com/studiomorfo)
        private const string ShopUrl = "https://www.shopier.com/studiomorfo";
        private const string CacheKey = "shopier_products";

        public ProductsController(IShopierScraper scraper, IMemoryCache cache)
        {
            _scraper = scraper;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            if (!_cache.TryGetValue(CacheKey, out IReadOnlyList<Product>? products))
            {
                products = await _scraper.GetProductsAsync(ShopUrl);

                // 10 dk cache
                _cache.Set(CacheKey, products, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });
            }

            return View(products ?? new List<Product>());
        }
    }
}