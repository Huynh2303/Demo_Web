namespace Demo_web_MVC.Service
{
    public interface IEmailServices
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
