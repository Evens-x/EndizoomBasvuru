using EndizoomBasvuru.Entity;
using EndizoomBasvuru.Services.Models;
using Microsoft.AspNetCore.Http;

namespace EndizoomBasvuru.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyResponseDto> RegisterCompanyAsync(CompanyRegisterDto model, int createdById);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<CompanyResponseDto?> GetCompanyByIdAsync(int id);
        Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync();
        Task<IEnumerable<CompanyResponseDto>> FilterCompaniesAsync(CompanyFilterDto filter);
        Task<CompanyLoginResponseDto?> AuthenticateAsync(LoginDto model);
        Task<bool> ChangePasswordAsync(int companyId, ChangePasswordDto model);
        Task<CompanyResponseDto> UpdateProfileAsync(int companyId, UpdateProfileDto model);
        Task<CompanyResponseDto?> GetCompanyProfileAsync(int companyId);
        Task<CompanyResponseDto> UpdateCompanyStatusAsync(int companyId, CompanyStatusUpdateDto model, int updatedById);
        Task<CompanyImageDto> AddCompanyImageAsync(int companyId, IFormFile image, string? description);
        Task<bool> DeleteCompanyImageAsync(int imageId, int companyId);
        Task<string> AddCompanyContractAsync(int companyId, IFormFile contract);
        Task<IEnumerable<Company>> FindByEmailAsync(string email);
        Task<CompanyLoginResponseDto?> ResetPasswordAsync(ResetPasswordDto model);

        // Yeni İstatistik Metodları
        Task<CompanyCountStatisticsDto> GetCompanyCountStatisticsAsync(StatisticPeriodType periodType, DateTime? date = null);
        Task<FinancialStatisticsDto> GetFinancialStatisticsAsync(StatisticPeriodType periodType, DateTime? date = null);
        Task<RecentCompaniesDto> GetRecentCompaniesAsync(int count = 10);
        Task<RecentCompaniesDto> GetNewCompaniesAsync(int days = 7, int count = 10);
        Task<PendingCompaniesDto> GetPendingCompaniesAsync(int count = 10);
    }
} 