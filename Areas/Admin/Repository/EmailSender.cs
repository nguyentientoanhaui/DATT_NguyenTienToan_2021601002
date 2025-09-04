using System.Net.Mail;
using System.Net;

namespace Shopping_Demo.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message, bool isHTML = true)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("dongianthoi631@gmail.com", "eeikbfkdhbcsyygp")
            };

            var mailMessage = new MailMessage(
                from: "dongianthoi631@gmail.com",
                to: email,
                subject,
                message
            );

            // Thiết lập định dạng HTML cho email
            mailMessage.IsBodyHtml = isHTML;

            return client.SendMailAsync(mailMessage);

        }
    }


}
