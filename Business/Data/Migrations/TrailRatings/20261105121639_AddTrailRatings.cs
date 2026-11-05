using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutdoorGuide.Business.Data.Migrations.TrailRatings
{
    /// <inheritdoc />
    public partial class AddTrailRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trailRating",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrailKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    MemberKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trailRating", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trailRating_TrailKey_MemberKey",
                table: "trailRating",
                columns: new[] { "TrailKey", "MemberKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trailRating");
        }
    }
}
