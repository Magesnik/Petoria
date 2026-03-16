using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Petoria.Infrastructure.Data;

/// <summary>
/// Design-time factory used exclusively by EF Core CLI tools (dotnet ef migrations add, etc.).
/// The connection string here is only used at design time and never in production.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(
            "Server=localhost;Database=petoria_design;User=root;Password=root;",
            new MySqlServerVersion(new Version(8, 0, 0)));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
