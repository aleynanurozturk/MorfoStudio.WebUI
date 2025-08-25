namespace MorfoStudio.WebUI.Models
{
    public class Product
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public decimal? Price { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
    }
}