using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdTorrBot.Migrations
{
    /// <inheritdoc />
    public partial class _201224 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerArgsConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdChat = table.Column<string>(type: "TEXT", nullable: false),
                    NameProfileBot = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: true),
                    HttpAuth = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReadOnlyMode = table.Column<bool>(type: "INTEGER", nullable: false),
                    Ssl = table.Column<bool>(type: "INTEGER", nullable: false),
                    SslPort = table.Column<int>(type: "INTEGER", nullable: true),
                    SslCert = table.Column<string>(type: "TEXT", nullable: true),
                    SslKey = table.Column<string>(type: "TEXT", nullable: true),
                    Path = table.Column<string>(type: "TEXT", nullable: true),
                    LogPath = table.Column<string>(type: "TEXT", nullable: true),
                    WebLogPath = table.Column<string>(type: "TEXT", nullable: true),
                    DontKill = table.Column<bool>(type: "INTEGER", nullable: false),
                    Ui = table.Column<bool>(type: "INTEGER", nullable: false),
                    TorrentsDir = table.Column<string>(type: "TEXT", nullable: true),
                    TorrentAddr = table.Column<string>(type: "TEXT", nullable: true),
                    PubIPv4 = table.Column<string>(type: "TEXT", nullable: true),
                    PubIPv6 = table.Column<string>(type: "TEXT", nullable: true),
                    SearchWa = table.Column<bool>(type: "INTEGER", nullable: false),
                    Help = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerArgsConfig", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerArgsConfig");
        }
    }
}
