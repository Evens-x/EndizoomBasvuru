using EndizoomBasvuru.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Security.Cryptography.X509Certificates;

namespace EndizoomBasvuru.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendWelcomeEmailAsync(string email, string name, string password)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["EmailSettings:SenderName"], _configuration["EmailSettings:SenderEmail"]));
            message.To.Add(new MailboxAddress(name, email));
            message.Subject = "Endizoom Başvuru Sistemine Hoş Geldiniz";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <h2>Endizoom Başvuru Sistemine Hoş Geldiniz!</h2>
                <p>Sayın {name},</p>
                <p>Endizoom Başvuru sistemine kaydınız başarıyla tamamlanmıştır.</p>
                <p>Sisteme giriş yapmak için kullanıcı bilgileriniz aşağıdadır:</p>
                <p><strong>E-posta:</strong> {email}<br>
                <strong>Şifre:</strong> {password}</p>
                <p style='color: red; font-weight: bold;'>Lütfen güvenliğiniz için en kısa sürede şifrenizi değiştirmeyi unutmayınız!</p>
                <p>Şifrenizi değiştirmek için, sisteme giriş yaptıktan sonra profil sayfanızdan şifre değiştirme işlemini gerçekleştirebilirsiniz.</p>
                <p>Saygılarımızla,<br>Endizoom Ekibi</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

        public async Task SendPasswordResetEmailAsync(string email, string name, string resetToken)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["EmailSettings:SenderName"], _configuration["EmailSettings:SenderEmail"]));
            message.To.Add(new MailboxAddress(name, email));
            message.Subject = "Şifre Sıfırlama";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <h2>Şifre Sıfırlama</h2>
                <p>Sayın {name},</p>
                <p>Şifrenizi sıfırlamak için aşağıdaki şifreyi kullanabilirsiniz:</p>
                <p><strong>Yeni Şifre:</strong> {resetToken}</p>
                <p>Güvenlik nedeniyle, giriş yaptıktan sonra şifrenizi değiştirmenizi öneririz.</p>
                <p>Saygılarımızla,<br>Endizoom Ekibi</p>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailAsync(message);
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            using var client = new SmtpClient();
            try
            {
                // SSL sertifikası doğrulamasını devre dışı bırak
                client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                await client.ConnectAsync(
                    _configuration["EmailSettings:SmtpServer"],
                    int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587"),
                    _configuration.GetValue<bool>("EmailSettings:EnableSsl") ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

                await client.AuthenticateAsync(_configuration["EmailSettings:SmtpUsername"], _configuration["EmailSettings:SmtpPassword"]);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                // Log exception details
                // In a production environment, consider using a proper logging framework
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
} 