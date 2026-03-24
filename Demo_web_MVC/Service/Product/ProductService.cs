using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Repository;
using Demo_web_MVC.Repository.Product;
namespace Demo_web_MVC.Service.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }       
        public async Task<ProductViewModel> details(int id)    
        {
            
            return await _productRepository.DetailsAsnyc(id);
        }
        public async Task<ProductViewModel> creat(ProductViewModel product)
        {
            return await _productRepository.AddAsnyc(product);
        }
        public async Task<ProductViewModel> update(int id, ProductViewModel product)
        {
            return await _productRepository.UpdateAsync(id, product);
        }
        public async Task<bool> delete(int id)
        {
            return await _productRepository.DeleteAsync(id);
        }
        public async Task<List<ProductViewModel>> getAll()
        {
            return await _productRepository.GetAllAsync();
        }
    }
}