using Microsoft.EntityFrameworkCore.Migrations;

namespace IotDash.Migrations
{
    public partial class renamehistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoryEntry_Interfaces_InterfaceId_DeviceId",
                table: "HistoryEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HistoryEntry",
                table: "HistoryEntry");

            migrationBuilder.RenameTable(
                name: "HistoryEntry",
                newName: "History");

            migrationBuilder.AddPrimaryKey(
                name: "PK_History",
                table: "History",
                columns: new[] { "InterfaceId", "DeviceId", "When" });

            migrationBuilder.AddForeignKey(
                name: "FK_History_Interfaces_InterfaceId_DeviceId",
                table: "History",
                columns: new[] { "InterfaceId", "DeviceId" },
                principalTable: "Interfaces",
                principalColumns: new[] { "Id", "DeviceId" },
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_History_Interfaces_InterfaceId_DeviceId",
                table: "History");

            migrationBuilder.DropPrimaryKey(
                name: "PK_History",
                table: "History");

            migrationBuilder.RenameTable(
                name: "History",
                newName: "HistoryEntry");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HistoryEntry",
                table: "HistoryEntry",
                columns: new[] { "InterfaceId", "DeviceId", "When" });

            migrationBuilder.AddForeignKey(
                name: "FK_HistoryEntry_Interfaces_InterfaceId_DeviceId",
                table: "HistoryEntry",
                columns: new[] { "InterfaceId", "DeviceId" },
                principalTable: "Interfaces",
                principalColumns: new[] { "Id", "DeviceId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
