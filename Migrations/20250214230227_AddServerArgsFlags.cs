using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdTorrBot.Migrations
{
    /// <inheritdoc />
    public partial class AddServerArgsFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettLogPath",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettPath",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettPort",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettPubIPv4",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettPubIPv6",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettSslCert",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettSslKey",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettSslPort",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettTorrentAddr",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettTorrentsDir",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagServerArgsSettWebLogPath",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettLogPath",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettPath",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettPort",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettPubIPv4",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettPubIPv6",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettSslCert",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettSslKey",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettSslPort",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettTorrentAddr",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettTorrentsDir",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagServerArgsSettWebLogPath",
                table: "TextInputFlag");
        }
    }
}
