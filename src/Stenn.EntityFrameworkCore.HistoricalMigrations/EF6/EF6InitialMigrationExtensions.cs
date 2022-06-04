using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Stenn.EntityFrameworkCore.HistoricalMigrations.EF6
{
    public static class EF6InitialMigrationExtensions
    {
        public static bool HasEF6InitialMigrationAttribute(this TypeInfo migration)
        {
            return migration.GetCustomAttribute<EF6InitialMigrationAttribute>() is not null;
        }

        public static EF6InitialMigrationAttribute GetEF6InitialMigrationAttribute(this TypeInfo migration)
        {
            return migration.GetCustomAttribute<EF6InitialMigrationAttribute>() ?? 
                   throw new InvalidOperationException();
        }

        
        public static bool EF6HistoryRepositoryExists(this IHistoryRepository repository, HistoryRepositoryDependencies dependencies)
        {
            //NOTE: Use hack with replace EF Core history table name to EF6 and only check exists
            var relationalOptions = RelationalOptionsExtension.Extract(dependencies.Options);
            var ef6RelationalOptions = relationalOptions.WithMigrationsHistoryTableName("__MigrationHistory");
            
            //NOTE: For replace relational options we must set exact type to WithExtension generic method
            var method = typeof(DbContextOptions).GetMethod(nameof(DbContextOptions.WithExtension))!;
            var generic = method.MakeGenericMethod(ef6RelationalOptions.GetType());
            var ef6DbContextOptions = (IDbContextOptions)generic.Invoke(dependencies.Options, new object?[] { ef6RelationalOptions })!;
            //var ef6DbContextOptions = ((DbContextOptions)dependencies.Options).WithExtension(ef6RelationalOptions);
            
            var ef6Dependincies = dependencies.With(ef6DbContextOptions);

            var ef6HistoryRepository = (IHistoryRepository?)Activator.CreateInstance(repository.GetType(), ef6Dependincies);

            if (ef6HistoryRepository == null)
            {
                throw new EF6MigrateException($"Can't create instance of {repository.GetType()}. Can't check exist EF6 history table or not");
            }
            return ef6HistoryRepository.Exists();
        }
        
        // public static bool EF6HistoryRepositoryExists(this ICurrentDbContext currentDbContext)
        // {
        //     
        //     
        //     var activeProvider = currentDbContext.Context.Database.ProviderName;
        //     var conn = currentDbContext.Context.Database.GetDbConnection();
        //         
        //     //TODO: Make extendable by Service
        //     switch (activeProvider)
        //     {
        //         case "Microsoft.EntityFrameworkCore.SqlServer":
        //         {
        //             cu
        //             
        //              "SELECT OBJECT_ID('[dbo].[__MigrationHistory]')"
        //                 + stringTypeMapping.GenerateSqlLiteral(
        //                     SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
        //                 + ")"
        //         }
        //             break;
        //         
        //     }
        // }
    }
}