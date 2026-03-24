using Demo_web_MVC.Repository.Product;
namespace Demo_web_MVC.Service.Product
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;
        public ProductService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public Task <object> Details (int? id)
        {
            throw new NotImplementedException();
            if (id == null)
            {
                return Task.FromResult<object>(new { success = false, message = "Không có sản phẩm" });
            }
            var product = _productRepository.GetAllAsync().Result.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                              return Task.FromResult<object>(new { success = false, message = "Không có sản phẩm" });
            }
            {
                                          
            }
        }

    }
}
