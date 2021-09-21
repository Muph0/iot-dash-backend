using Microsoft.EntityFrameworkCore.Migrations;

namespace IotDash.Migrations
{
    public partial class Add_LogHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LogHistory",
                table: "Interfaces",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogHistory",
                table: "Interfaces");
        }
    }
}
