using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobType",
                schema: "ticker",
                table: "TimeTickers",
                type: "TEXT",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobType",
                schema: "ticker",
                table: "CronTickers",
                type: "TEXT",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTickers_JobType",
                schema: "ticker",
                table: "TimeTickers",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_CronTickers_JobType",
                schema: "ticker",
                table: "CronTickers",
                column: "JobType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeTickers_JobType",
                schema: "ticker",
                table: "TimeTickers");

            migrationBuilder.DropIndex(
                name: "IX_CronTickers_JobType",
                schema: "ticker",
                table: "CronTickers");

            migrationBuilder.DropColumn(
                name: "JobType",
                schema: "ticker",
                table: "TimeTickers");

            migrationBuilder.DropColumn(
                name: "JobType",
                schema: "ticker",
                table: "CronTickers");
        }
    }
}
