using Microsoft.EntityFrameworkCore.Migrations;

namespace IotDash.Migrations
{
    public partial class ChangeToUtc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "When",
                table: "History",
                newName: "WhenUTC");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhenUTC",
                table: "History",
                newName: "When");
        }
    }
}
