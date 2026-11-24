using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutdoorGuide.Business.Data.Migrations.Newsletter
{
    /// <inheritdoc />
    public partial class AddNewsletterSends : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "newsletterDelivery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NewsletterKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false),
                    SentUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_newsletterDelivery", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "newsletterSend",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NewsletterKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    QueuedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_newsletterSend", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_newsletterDelivery_NewsletterKey_SubscriptionId",
                table: "newsletterDelivery",
                columns: new[] { "NewsletterKey", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_newsletterSend_NewsletterKey",
                table: "newsletterSend",
                column: "NewsletterKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "newsletterDelivery");

            migrationBuilder.DropTable(
                name: "newsletterSend");
        }
    }
}
