using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConnectorAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixTraversingRelationalData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Connections_connectionId",
                table: "Resources");

            migrationBuilder.RenameColumn(
                name: "connectionId",
                table: "Resources",
                newName: "ConnectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Resources_connectionId",
                table: "Resources",
                newName: "IX_Resources_ConnectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Connections_ConnectionId",
                table: "Resources",
                column: "ConnectionId",
                principalTable: "Connections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Connections_ConnectionId",
                table: "Resources");

            migrationBuilder.RenameColumn(
                name: "ConnectionId",
                table: "Resources",
                newName: "connectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Resources_ConnectionId",
                table: "Resources",
                newName: "IX_Resources_connectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Connections_connectionId",
                table: "Resources",
                column: "connectionId",
                principalTable: "Connections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
