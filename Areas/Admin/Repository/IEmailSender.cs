namespace Shopping_Demo.Areas.Admin.Repository
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message, bool isHTML = true);
    }
}
