using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaCountComputedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MediaCount",
                table: "PlexLibraries",
                type: "INTEGER",
                nullable: false,
                computedColumnSql: "CASE WHEN Type = 'Movie' THEN MovieCount WHEN Type = 'TvShow' THEN TvShowCount ELSE -1 END"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "MediaCount", table: "PlexLibraries");
        }
    }
}
