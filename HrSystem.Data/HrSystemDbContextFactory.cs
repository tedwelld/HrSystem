using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HrSystem.Data;

public class HrSystemDbContextFactory : IDesignTimeDbContextFactory<HrSystemDbContext>
{
    public HrSystemDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HrSystemDbContext>();
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HrSystemDb;MultipleActiveResultSets=true;TrustServerCertificate=true;");
        return new HrSystemDbContext(optionsBuilder.Options);
    }
}


