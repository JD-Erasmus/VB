using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VB.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVaultShares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VaultShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VaultId = table.Column<int>(type: "int", nullable: false),
                    OwnerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EncryptedPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientNote = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    MaxViews = table.Column<int>(type: "int", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    FirstViewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaultShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaultShares_Vault_VaultId",
                        column: x => x.VaultId,
                        principalTable: "Vault",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VaultShares_TokenHash",
                table: "VaultShares",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaultShares_VaultId_OwnerUserId",
                table: "VaultShares",
                columns: new[] { "VaultId", "OwnerUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VaultShares");
        }
    }
}
