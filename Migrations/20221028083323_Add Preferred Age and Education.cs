using Microsoft.EntityFrameworkCore.Migrations;

namespace JobPortal.Migrations
{
    public partial class AddPreferredAgeandEducation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "Jobs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PreferredAge",
                table: "Jobs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Education",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PreferredAge",
                table: "Jobs");
        }
    }
}
