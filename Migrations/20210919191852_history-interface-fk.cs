using Microsoft.EntityFrameworkCore.Migrations;

namespace IotDash.Migrations
{
    public partial class historyinterfacefk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_HistoryEntry_Interfaces_InterfaceId_DeviceId",
                table: "HistoryEntry",
                columns: new[] { "InterfaceId", "DeviceId" },
                principalTable: "Interfaces",
                principalColumns: new[] { "Id", "DeviceId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoryEntry_Interfaces_InterfaceId_DeviceId",
                table: "HistoryEntry");
        }
    }
}
