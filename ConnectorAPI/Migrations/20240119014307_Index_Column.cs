using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConnectorAPI.Migrations
{
    /// <inheritdoc />
    public partial class Index_Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ResourceName",
                table: "Resources",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ResourceId",
                table: "Resources",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerNode",
                table: "Connections",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AccessorNode",
                table: "Connections",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourceId",
                table: "Resources",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourceName",
                table: "Resources",
                column: "ResourceName");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_OwnerNode_AccessorNode",
                table: "Connections",
                columns: new[] { "OwnerNode", "AccessorNode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Resources_ResourceId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_ResourceName",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Connections_OwnerNode_AccessorNode",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "Resources");

            migrationBuilder.AlterColumn<string>(
                name: "ResourceName",
                table: "Resources",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerNode",
                table: "Connections",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "AccessorNode",
                table: "Connections",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
