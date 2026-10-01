using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models.Data;

public class EncryptedAppDbContext : DbContext
{
    public EncryptedAppDbContext(DbContextOptions<EncryptedAppDbContext> options)
        : base(options)
    {
    }
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
}
