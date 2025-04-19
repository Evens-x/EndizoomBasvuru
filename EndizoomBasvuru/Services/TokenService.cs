using EndizoomBasvuru.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EndizoomBasvuru.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateCompanyToken(Company company)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, company.Id.ToString()),
                new Claim(ClaimTypes.Email, company.Email),
                new Claim(ClaimTypes.Name, $"{company.ContactFirstName} {company.ContactLastName}"),
                new Claim("CompanyName", company.Name),
                new Claim(ClaimTypes.Role, UserRole.Company.ToString()),
                new Claim("role", UserRole.Company.ToString())
            };

            return GenerateToken(claims);
        }

        public string GenerateAdminToken(Admin admin)
        {
            var roleName = admin.Role.ToString();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim(ClaimTypes.Name, $"{admin.FirstName} {admin.LastName}"),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("role", roleName),
                new Claim("admin_role", roleName)
            };

            if (roleName == UserRole.Admin.ToString())
            {
                claims.Add(new Claim("is_admin", "true"));
            } 
            else if (roleName == UserRole.Marketing.ToString())
            {
                claims.Add(new Claim("is_marketing", "true"));
            }

            return GenerateToken(claims);
        }

        private string GenerateToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "EndizoomDefaultSecureKeyForDevelopment1234"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var expires = DateTime.Now.AddDays(30);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}