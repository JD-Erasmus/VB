using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VB.Data.Migrations
{
    public partial class AddVaultUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Vault",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Vault_UserId",
                table: "Vault",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vault_AspNetUsers_UserId",
                table: "Vault",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vault_AspNetUsers_UserId",
                table: "Vault");

            migrationBuilder.DropIndex(
                name: "IX_Vault_UserId",
                table: "Vault");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Vault");
        }
    }
}
