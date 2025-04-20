using EndizoomBasvuru.Entity;
using EndizoomBasvuru.Services.Models;

namespace EndizoomBasvuru.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminLoginResponseDto?> AuthenticateAsync(LoginDto model);
        Task<AdminResponseDto> RegisterAdminAsync(AdminRegisterDto model, int createdById);
        Task<IEnumerable<AdminResponseDto>> GetAllAdminsAsync();
        Task<AdminResponseDto?> GetAdminByIdAsync(int id);
        Task<AdminResponseDto> UpdateAdminAsync(int id, AdminUpdateDto model);
        Task<bool> DeleteAdminAsync(int id);
        Task<bool> ChangePasswordAsync(int adminId, ChangePasswordDto model);
        Task<IEnumerable<AdminResponseDto>> GetAdminsByRoleAsync(UserRole role);
        Task<IEnumerable<AdminResponseDto>> GetAdminAndMarketingUsersAsync();
        
        // Yeni metotlar
        Task<AdminResponseDto> UpdateAdminStatusAsync(int id, AdminStatusUpdateDto model);
        Task<MarketingUserStatsDto> GetMarketingUserStatsAsync(int userId, DateTime? date = null);
        Task<IEnumerable<MarketingUserStatsDto>> GetAllMarketingUsersStatsAsync(DateTime? date = null);
    }
} 