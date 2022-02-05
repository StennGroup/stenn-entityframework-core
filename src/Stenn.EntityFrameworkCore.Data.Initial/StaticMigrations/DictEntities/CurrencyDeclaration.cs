using System.Collections.Generic;

namespace Stenn.EntityFrameworkCore.Data.Initial.StaticMigrations.DictEntities
{
    public static class CurrencyDeclaration
    {
        public static List<Currency> GetActual()
        {
            return new List<Currency>
            {
                Currency.Create(1, "TST", 2, "Test currency"),
                Currency.Create(2, "TS2", 2, "Test currency 2")
            };
        }
    }
}