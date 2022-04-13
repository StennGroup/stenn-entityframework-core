using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Stenn.EntityFrameworkCore.Data.Main.StaticMigrations;
using Stenn.EntityFrameworkCore.EntityConventions;
using Stenn.EntityFrameworkCore.SqlServer.Extensions.DependencyInjection;

namespace Stenn.EntityFrameworkCore.Data.Main
{
    // ReSharper disable once UnusedType.Global
    public class MainDbContextFactory : IDesignTimeDbContextFactory<MainDbContext>
    {
        public MainDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MainDbContext>();
            
            optionsBuilder.UseSqlServer();
            optionsBuilder.UseStaticMigrationsSqlServer(options =>
                {
                    options.InitMigrations = MainStaticMigrations.Init;
                    options.ConventionsOptions.InitEntityConventions = b => { b.AddTriggerBasedCommonConventions(); };
                }
            );
            
            return new MainDbContext(optionsBuilder.Options);
        }
    }
}