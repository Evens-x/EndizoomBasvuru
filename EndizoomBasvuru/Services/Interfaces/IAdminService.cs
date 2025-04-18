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
    }
} 