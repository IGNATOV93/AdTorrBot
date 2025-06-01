using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdTorrBot.Migrations
{
    /// <inheritdoc />
    public partial class _1311 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettCacheSize",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettConnectionsLimit",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettDownloadRateLimit",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettFriendlyName",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettPeersListenPort",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettPreloadCache",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettReaderReadAHead",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettRetrackersMode",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettSslCert",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettSslKey",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettSslPort",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettTorrentDisconnectTimeout",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettTorrentsSavePath",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FlagTorrSettUploadRateLimit",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlagTorrSettCacheSize",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettConnectionsLimit",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettDownloadRateLimit",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettFriendlyName",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettPeersListenPort",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettPreloadCache",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettReaderReadAHead",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettRetrackersMode",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettSslCert",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettSslKey",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettSslPort",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettTorrentDisconnectTimeout",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettTorrentsSavePath",
                table: "TextInputFlag");

            migrationBuilder.DropColumn(
                name: "FlagTorrSettUploadRateLimit",
                table: "TextInputFlag");
        }
    }
}
