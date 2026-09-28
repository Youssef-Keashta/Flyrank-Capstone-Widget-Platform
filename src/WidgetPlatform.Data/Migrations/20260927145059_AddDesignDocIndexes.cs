using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WidgetPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignDocIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Widgets_OwnerId",
                table: "Widgets",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_OwnerId_CreatedAt",
                table: "Submissions",
                columns: new[] { "OwnerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_WidgetId",
                table: "Submissions",
                column: "WidgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_WidgetId_IdempotencyKey",
                table: "Submissions",
                columns: new[] { "WidgetId", "IdempotencyKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Widgets_OwnerId",
                table: "Widgets");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_OwnerId_CreatedAt",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_WidgetId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_WidgetId_IdempotencyKey",
                table: "Submissions");
        }
    }
}
