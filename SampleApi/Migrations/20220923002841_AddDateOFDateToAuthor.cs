using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SampleApi.Migrations
{
    public partial class AddDateOFDateToAuthor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DateOfDeath",
                table: "Authors",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfDeath",
                table: "Authors");
        }
    }
}
