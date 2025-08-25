using Microsoft.Extensions.Configuration;
using MorfoStudio.WebUI.Models;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
namespace MorfoStudio.WebUI.Services
{
    public class ShopierScraper : IShopierScraper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ShopierScraper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync(string shopUrl)
        {
            var http = _httpClientFactory.CreateClient(nameof(ShopierScraper));
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

            var html = await http.GetStringAsync(shopUrl);

            // AngleSharp ile parse et
            var context = BrowsingContext.New(Configuration.Default);
            var doc = await context.OpenAsync(req => req.Content(html));

            // Kart seçicisi (senin bıraktığın HTML’e göre)
            var cards = doc.QuerySelectorAll(".product-card.shopier--product-card.product-card-store");

            var tr = new CultureInfo("tr-TR");
            var list = new List<Product>();

            foreach (var card in cards)
            {
                try
                {
                    var a = card.QuerySelector("a.product-image-link-store");
                    if (a == null) continue;

                    var url = a.GetAttribute("href")?.Trim();
                    if (string.IsNullOrWhiteSpace(url)) continue;

                    // ID: data-back-id veya URL içinden
                    var id = a.GetAttribute("data-back-id")?.Trim();
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        var parts = new Uri(url, UriKind.Absolute).Segments;
                        id = parts.LastOrDefault()?.Trim('/');
                    }

                    // Başlık
                    var title = card.QuerySelector(".product-card-title h3")?.TextContent?.Trim();
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        // fallback: img title
                        title = card.QuerySelector("picture img")?.GetAttribute("title")?.Trim()
                             ?? card.QuerySelector("picture img")?.GetAttribute("alt")?.Trim()
                             ?? "Ürün";
                    }

                    // Resim: src varsa onu, yoksa srcset’ten büyük görseli al
                    var img = card.QuerySelector("picture img");
                    var imgSrc = img?.GetAttribute("src")?.Trim();
                    var srcSet = img?.GetAttribute("srcset")?.Trim();

                    string imageUrl = imgSrc ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(imageUrl) && !string.IsNullOrWhiteSpace(srcSet))
                    {
                        imageUrl = srcSet.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
                    }

                    // Fiyat: .price-value ve .price-currency
                    var priceText = card.QuerySelector(".product-card-price .price .price-value")?.TextContent?.Trim();
                    var currency = card.QuerySelector(".product-card-price .price .price-currency")?.TextContent?.Trim();

                    decimal? price = null;
                    if (!string.IsNullOrWhiteSpace(priceText))
                    {
                        if (decimal.TryParse(priceText, NumberStyles.Number, tr, out var p))
                            price = p;
                        else if (decimal.TryParse(priceText.Replace(".", "").Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out var p2))
                            price = p2;
                    }

                    list.Add(new Product
                    {
                        Id = id ?? Guid.NewGuid().ToString("N"),
                        Title = title ?? "Ürün",
                        ImageUrl = imageUrl,
                        Url = url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                ? url
                                : new Uri(new Uri(shopUrl), url).ToString(),
                        Price = price,
                        Currency = string.IsNullOrWhiteSpace(currency) ? "TL" : currency
                    });
                }
                catch
                {
                    // tek ürün patlarsa tüm listeyi bozdurmayalım
                    continue;
                }
            }

            return list;
        }
    }
}