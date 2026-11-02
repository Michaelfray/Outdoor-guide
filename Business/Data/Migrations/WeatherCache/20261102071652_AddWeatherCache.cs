using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutdoorGuide.Business.Data.Migrations.WeatherCache
{
    /// <inheritdoc />
    public partial class AddWeatherCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "weatherCache",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrailKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    ForecastJson = table.Column<string>(type: "TEXT", nullable: false),
                    FetchedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weatherCache", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_weatherCache_TrailKey",
                table: "weatherCache",
                column: "TrailKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weatherCache");
        }
    }
}
