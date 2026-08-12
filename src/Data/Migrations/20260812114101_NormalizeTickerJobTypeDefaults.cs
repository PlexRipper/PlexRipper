using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reaparr.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeTickerJobTypeDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobType",
                schema: "ticker",
                table: "TimeTickers",
                type: "TEXT",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "None",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "JobType",
                schema: "ticker",
                table: "CronTickers",
                type: "TEXT",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "None",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobType",
                schema: "ticker",
                table: "TimeTickers",
                type: "TEXT",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 100,
                oldDefaultValue: "None");

            migrationBuilder.AlterColumn<string>(
                name: "JobType",
                schema: "ticker",
                table: "CronTickers",
                type: "TEXT",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldUnicode: false,
                oldMaxLength: 100,
                oldDefaultValue: "None");
        }
    }
}
