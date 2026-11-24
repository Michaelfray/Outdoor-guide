using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutdoorGuide.Business.Data.Migrations.Newsletter
{
    /// <inheritdoc />
    public partial class AddUnsubscribeToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnsubscribeToken",
                table: "newsletterSubscription",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // De som redan prenumererar får varsin slumpad token. Annars stoppar det unika indexet migrationen (OUT-118)
            migrationBuilder.Sql("UPDATE newsletterSubscription SET UnsubscribeToken = lower(hex(randomblob(32))) WHERE UnsubscribeToken = '';");

            migrationBuilder.CreateIndex(
                name: "IX_newsletterSubscription_UnsubscribeToken",
                table: "newsletterSubscription",
                column: "UnsubscribeToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_newsletterSubscription_UnsubscribeToken",
                table: "newsletterSubscription");

            migrationBuilder.DropColumn(
                name: "UnsubscribeToken",
                table: "newsletterSubscription");
        }
    }
}
