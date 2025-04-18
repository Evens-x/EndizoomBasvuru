namespace EndizoomBasvuru.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string email, string name, string password);
        Task SendPasswordResetEmailAsync(string email, string name, string resetToken);
    }
} 