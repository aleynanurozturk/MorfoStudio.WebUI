using MorfoStudio.WebUI.Models;

namespace MorfoStudio.WebUI.Services
{
    public interface IShopierScraper
    {
        Task<IReadOnlyList<Product>> GetProductsAsync(string shopUrl);
    }
}
