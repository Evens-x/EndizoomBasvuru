using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EndizoomBasvuru.Services.Interfaces;
using EndizoomBasvuru.Services.Models;
using EndizoomBasvuru.Entity;
using System.Security.Claims;

namespace EndizoomBasvuru.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
    public class StatisticsController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public StatisticsController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        /// <summary>
        /// Günlük firma ekleme istatistiklerini getirir
        /// </summary>
        [HttpGet("companies/daily")]
        public async Task<IActionResult> GetDailyCompanyStatistics([FromQuery] DateTime? date = null)
        {
            var statistics = await _companyService.GetCompanyCountStatisticsAsync(StatisticPeriodType.Daily, date);
            return Ok(statistics);
        }

        /// <summary>
        /// Aylık firma ekleme istatistiklerini getirir
        /// </summary>
        [HttpGet("companies/monthly")]
        public async Task<IActionResult> GetMonthlyCompanyStatistics([FromQuery] DateTime? date = null)
        {
            var statistics = await _companyService.GetCompanyCountStatisticsAsync(StatisticPeriodType.Monthly, date);
            return Ok(statistics);
        }

        /// <summary>
        /// Günlük finansal istatistikleri getirir
        /// </summary>
        [HttpGet("financial/daily")]
        public async Task<IActionResult> GetDailyFinancialStatistics([FromQuery] DateTime? date = null)
        {
            var statistics = await _companyService.GetFinancialStatisticsAsync(StatisticPeriodType.Daily, date);
            return Ok(statistics);
        }

        /// <summary>
        /// Aylık finansal istatistikleri getirir
        /// </summary>
        [HttpGet("financial/monthly")]
        public async Task<IActionResult> GetMonthlyFinancialStatistics([FromQuery] DateTime? date = null)
        {
            var statistics = await _companyService.GetFinancialStatisticsAsync(StatisticPeriodType.Monthly, date);
            return Ok(statistics);
        }

        /// <summary>
        /// Son eklenen firmaları getirir
        /// </summary>
        [HttpGet("companies/recent")]
        public async Task<IActionResult> GetRecentCompanies([FromQuery] int count = 10)
        {
            var companies = await _companyService.GetRecentCompaniesAsync(count);
            return Ok(companies);
        }

        /// <summary>
        /// Yeni eklenen firmaları getirir (son X gün)
        /// </summary>
        [HttpGet("companies/new")]
        public async Task<IActionResult> GetNewCompanies([FromQuery] int days = 7, [FromQuery] int count = 10)
        {
            var companies = await _companyService.GetNewCompaniesAsync(days, count);
            return Ok(companies);
        }

        /// <summary>
        /// Onay bekleyen firmaları getirir
        /// </summary>
        [HttpGet("companies/pending")]
        public async Task<IActionResult> GetPendingCompanies([FromQuery] int count = 10)
        {
            var companies = await _companyService.GetPendingCompaniesAsync(count);
            return Ok(companies);
        }
    }
} 