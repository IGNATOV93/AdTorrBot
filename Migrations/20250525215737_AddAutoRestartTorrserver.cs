using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdTorrBot.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoRestartTorrserver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTorrserverAutoRestart",
                table: "SettingsTorrserverBot",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TorrserverRestartTime",
                table: "SettingsTorrserverBot",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTorrserverAutoRestart",
                table: "SettingsTorrserverBot");

            migrationBuilder.DropColumn(
                name: "TorrserverRestartTime",
                table: "SettingsTorrserverBot");
        }
    }
}
