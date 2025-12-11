using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Semester_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityUserIdToISPUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "ISP_Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "ISP_Users");
        }
    }
}
