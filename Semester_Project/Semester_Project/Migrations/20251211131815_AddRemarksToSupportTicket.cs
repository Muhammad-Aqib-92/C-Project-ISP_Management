using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Semester_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddRemarksToSupportTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "SupportTickets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "SupportTickets");
        }
    }
}
