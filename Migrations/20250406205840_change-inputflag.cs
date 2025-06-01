using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdTorrBot.Migrations
{
    /// <inheritdoc />
    public partial class changeinputflag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FlagNoteOtherProfile",
                table: "TextInputFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlagNoteOtherProfile",
                table: "TextInputFlag");
        }
    }
}
