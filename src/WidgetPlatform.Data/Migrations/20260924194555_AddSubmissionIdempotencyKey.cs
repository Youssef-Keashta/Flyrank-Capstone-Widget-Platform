using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WidgetPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Submissions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Submissions");
        }
    }
}
