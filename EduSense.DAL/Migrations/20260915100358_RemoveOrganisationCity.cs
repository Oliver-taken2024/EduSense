using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSense.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrganisationCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Organisation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Organisation",
                type: "text",
                nullable: true);
        }
    }
}
