using Microsoft.EntityFrameworkCore.Migrations;

namespace JobPortal.Migrations
{
    public partial class Pagination : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "P",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "S",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "P",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "S",
                table: "Jobs");
        }
    }
}
