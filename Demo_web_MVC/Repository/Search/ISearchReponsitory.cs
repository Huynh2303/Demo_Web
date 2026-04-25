using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Models.ViewModel.Search;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace Demo_web_MVC.Repository.Search
{
    
    
    public interface ISearchReponsitory
    {
        Task<SearchViewModel> SearchAsync(string searchQuery);

        
    }
}
