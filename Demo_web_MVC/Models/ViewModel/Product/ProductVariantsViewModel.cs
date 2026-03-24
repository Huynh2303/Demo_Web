namespace Demo_web_MVC.Models.ViewModel.Product
{
    public class ProductVariantsViewModel
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public string? Size { get; set; }

        public string? Color { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }
    }
}
