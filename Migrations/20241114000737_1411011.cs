using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdTorrBot.Migrations
{
    /// <inheritdoc />
    public partial class _1411011 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BitTorrConfig_IdChat_NameProfileBot",
                table: "BitTorrConfig",
                columns: new[] { "IdChat", "NameProfileBot" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BitTorrConfig_IdChat_NameProfileBot",
                table: "BitTorrConfig");
        }
    }
}
