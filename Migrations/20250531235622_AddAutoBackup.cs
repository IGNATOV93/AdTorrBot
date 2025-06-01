using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdTorrBot.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoBackup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AutoBackupTime",
                table: "SettingsTorrserverBot",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoBackupEnabled",
                table: "SettingsTorrserverBot",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoBackupTime",
                table: "SettingsTorrserverBot");

            migrationBuilder.DropColumn(
                name: "IsAutoBackupEnabled",
                table: "SettingsTorrserverBot");
        }
    }
}
