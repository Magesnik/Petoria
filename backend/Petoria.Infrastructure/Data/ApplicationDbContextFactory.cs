using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Petoria.Infrastructure.Data;

/// <summary>
/// Фабрика за създаване на DbContext при design-time (използва се от EF Core CLI — dotnet ef migrations add и т.н.).
/// Connection string-ът тук се използва само при генериране на миграции, не в продукция.
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
