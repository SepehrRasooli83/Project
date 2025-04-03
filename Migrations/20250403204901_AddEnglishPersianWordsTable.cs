using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishPersianWordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersianEnglishWordss",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IDdataroot = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnglishWord = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersianWord = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersianEnglishWordss", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersianEnglishWordss");
        }
    }
}
