using EndizoomBasvuru.Data.Context;
using EndizoomBasvuru.Entity;
using EndizoomBasvuru.Repository.Interfaces;

namespace EndizoomBasvuru.Repository
{
    public class CompanyImageRepository : GenericRepository<CompanyImage>, ICompanyImageRepository
    {
        public CompanyImageRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
} 