using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Semester_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageExpiryDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PackageExpiryDate",
                table: "ISP_Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackageExpiryDate",
                table: "ISP_Users");
        }
    }
}
