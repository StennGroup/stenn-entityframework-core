﻿using Stenn.EntityFrameworkCore.Data.Initial.StaticMigrations.DictEntities;
using Stenn.EntityFrameworkCore.Extensions.DependencyInjection;

namespace Stenn.EntityFrameworkCore.Data.Initial.StaticMigrations
{
    public static class InitialStaticMigrations
    {
        public static void Init(StaticMigrationBuilder migrations)
        {
            migrations.AddInitResSql("InitDB", @"\StaticMigrations\Sql\InitDB.Apply.sql", suppressTransaction: true);

            migrations.AddResSql("TestViews", @"\StaticMigrations\Sql\TestViews.Apply.sql", @"StaticMigrations\Sql\TestViews.Revert.sql");

            migrations.AddDictionaryEntity(CurrencyDeclaration.GetActual);
        }
    }
}