using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Stenn.EntityFrameworkCore.EntityConventions
{
    public class RelationalDbContextOptionsConfigurator : IDbContextOptionsConfigurator
    {
        /// <inheritdoc />
        public void Configure(DbContextOptionsBuilder builder)
        {
            builder.ReplaceService<IModelCustomizer, RelationalModelCustomizerWithConventions>();
        }
    }
}