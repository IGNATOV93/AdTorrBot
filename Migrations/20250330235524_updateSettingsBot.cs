using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdTorrBot.Migrations
{
    /// <inheritdoc />
    public partial class updateSettingsBot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FlagLoginPasswordOtherProfile",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastChangeUid",
                table: "SettingsBot",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlagLoginPasswordOtherProfile",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "LastChangeUid",
                table: "SettingsBot");
        }
    }
}
