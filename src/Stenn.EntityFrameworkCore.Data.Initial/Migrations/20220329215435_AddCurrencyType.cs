﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Stenn.EntityFrameworkCore.DbContext.Initial.Migrations
{
    public partial class AddCurrencyType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Currency",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Currency");
        }
    }
}
