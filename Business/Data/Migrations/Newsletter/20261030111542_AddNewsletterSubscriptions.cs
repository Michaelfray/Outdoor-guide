using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutdoorGuide.Business.Data.Migrations.Newsletter
{
    /// <inheritdoc />
    public partial class AddNewsletterSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "newsletterSubscription",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false),
                    Culture = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_newsletterSubscription", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_newsletterSubscription_Email",
                table: "newsletterSubscription",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "newsletterSubscription");
        }
    }
}
