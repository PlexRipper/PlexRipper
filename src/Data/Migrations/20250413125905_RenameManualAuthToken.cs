using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlexRipper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameManualAuthToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ManualAuthenticationToken",
                table: "PlexAccounts",
                newName: "CustomAuthenticationToken"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomAuthenticationToken",
                table: "PlexAccounts",
                newName: "ManualAuthenticationToken"
            );
        }
    }
}
