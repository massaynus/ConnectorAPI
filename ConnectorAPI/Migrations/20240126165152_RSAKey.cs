using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConnectorAPI.Migrations
{
    /// <inheritdoc />
    public partial class RSAKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RSAPublicKey",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RSAPublicKey",
                table: "AspNetUsers");
        }
    }
}
