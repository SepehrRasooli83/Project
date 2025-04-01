using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWordUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EnglishWords_Word",
                table: "EnglishWords");

            migrationBuilder.CreateIndex(
                name: "IX_EnglishWords_Word",
                table: "EnglishWords",
                column: "Word");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EnglishWords_Word",
                table: "EnglishWords");

            migrationBuilder.CreateIndex(
                name: "IX_EnglishWords_Word",
                table: "EnglishWords",
                column: "Word",
                unique: true);
        }
    }
}
