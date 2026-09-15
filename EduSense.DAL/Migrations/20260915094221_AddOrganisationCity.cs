using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSense.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationCity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Segment",
                table: "Respondent",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Organisation",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Segment",
                table: "Respondent");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Organisation");
        }
    }
}
