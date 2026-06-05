using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ATM.Infrastructure.Data;

/// <summary>
/// Tasarım zamanı (dotnet ef) araçlarının çalışan bir veritabanına ihtiyaç duymadan
/// AppDbContext oluşturabilmesi için kullanılır. Yalnızca migration üretiminde devreye girer.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=atm;Username=atm;Password=atm")
            .Options;

        return new AppDbContext(options);
    }
}
