﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Stenn.EntityFrameworkCore.Data.Initial;
using Stenn.EntityFrameworkCore.DbContext.Initial;

namespace Stenn.EntityFrameworkCore.DbContext.Initial.Migrations
{
    [DbContext(typeof(InitialDbContext))]
    [Migration("20220122095651_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.13")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Stenn.EntityFrameworkCore.Data.Currency", b =>
                {
                    b.Property<string>("Iso3LetterCode")
                        .HasMaxLength(3)
                        .IsUnicode(false)
                        .HasColumnType("char(3)")
                        .IsFixedLength(true);

                    b.Property<byte>("DecimalDigits")
                        .HasColumnType("tinyint");

                    b.Property<string>("Description")
                        .HasMaxLength(150)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(150)");

                    b.Property<int>("IsoNumericCode")
                        .HasColumnType("int");

                    b.HasKey("Iso3LetterCode");

                    b.ToTable("Currency");
                });
#pragma warning restore 612, 618
        }
    }
}
