using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    }
}
