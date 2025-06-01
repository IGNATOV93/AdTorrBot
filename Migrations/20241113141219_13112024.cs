using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdTorrBot.Migrations
{
    /// <inheritdoc />
    public partial class _13112024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BitTorrConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdChat = table.Column<string>(type: "TEXT", nullable: false),
                    NameProfileBot = table.Column<string>(type: "TEXT", nullable: false),
                    CacheSize = table.Column<long>(type: "INTEGER", nullable: false),
                    ReaderReadAHead = table.Column<int>(type: "INTEGER", nullable: false),
                    PreloadCache = table.Column<int>(type: "INTEGER", nullable: false),
                    UseDisk = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableIPv6 = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableTCP = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableUTP = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisablePEX = table.Column<bool>(type: "INTEGER", nullable: false),
                    ForceEncrypt = table.Column<bool>(type: "INTEGER", nullable: false),
                    TorrentDisconnectTimeout = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectionsLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    DisableDHT = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadRateLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadRateLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    PeersListenPort = table.Column<int>(type: "INTEGER", nullable: false),
                    DisableUPNP = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableDLNA = table.Column<bool>(type: "INTEGER", nullable: false),
                    FriendlyName = table.Column<string>(type: "TEXT", nullable: false),
                    EnableRutorSearch = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableDebug = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResponsiveMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    RetrackersMode = table.Column<int>(type: "INTEGER", nullable: false),
                    SslPort = table.Column<int>(type: "INTEGER", nullable: false),
                    SslCert = table.Column<string>(type: "TEXT", nullable: false),
                    SslKey = table.Column<string>(type: "TEXT", nullable: false),
                    DisableUpload = table.Column<bool>(type: "INTEGER", nullable: false),
                    RemoveCacheOnDrop = table.Column<bool>(type: "INTEGER", nullable: false),
                    TorrentsSavePath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitTorrConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SettingsBot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdChat = table.Column<string>(type: "TEXT", nullable: false),
                    TimeZoneOffset = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingsBot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SettingsTorrserverBot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    idChat = table.Column<string>(type: "TEXT", nullable: true),
                    Login = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    IsActiveAutoChange = table.Column<bool>(type: "INTEGER", nullable: false),
                    TimeAutoChangePassword = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingsTorrserverBot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TextInputFlag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdChat = table.Column<string>(type: "TEXT", nullable: true),
                    FlagLogin = table.Column<bool>(type: "INTEGER", nullable: false),
                    FlagPassword = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextInputFlag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdChat = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BitTorrConfig");

            migrationBuilder.DropTable(
                name: "SettingsBot");

            migrationBuilder.DropTable(
                name: "SettingsTorrserverBot");

            migrationBuilder.DropTable(
                name: "TextInputFlag");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
