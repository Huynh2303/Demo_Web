using Demo_web_MVC.Models.ViewModel;

namespace Demo_web_MVC.Service.auth
{
    public interface IAuthService
    {
         Task<bool> LoginService(LoginViewModel model);
    }
}
