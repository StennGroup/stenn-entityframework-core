﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Stenn.EntityFrameworkCore.EntityConventions
{
    public class DbContextOptionsConfigurator : IDbContextOptionsConfigurator
    {
        /// <inheritdoc />
        public void Configure(DbContextOptionsBuilder builder)
        {
            builder.ReplaceService<IModelCustomizer, ModelCustomizerWithConventions>();
        }
    }
}