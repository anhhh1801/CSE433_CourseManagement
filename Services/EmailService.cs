using CourseManagement.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace CourseManagement.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> options)
        {
            _smtp = options.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var mail = new MailMessage(_smtp.Username, to, subject, body);
            mail.IsBodyHtml = true;

            using var smtp = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
    }
}
