﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Stenn.EntityFrameworkCore.Data.Main.Configurations
{
    public class CurrencyMap: IEntityTypeConfiguration<Currency>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            builder.Property(x => x.Iso3LetterCode).IsUnicode(false).HasMaxLength(3).IsFixedLength().IsRequired();
            builder.HasKey(x => x.Iso3LetterCode);
            
            builder.Property(x => x.IsoNumericCode).IsRequired();
            builder.Property(x => x.DecimalDigits).IsRequired();
            builder.Property(x => x.Description).IsUnicode().HasMaxLength(150);
        }
    }
}