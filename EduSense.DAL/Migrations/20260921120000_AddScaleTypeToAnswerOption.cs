using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSense.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddScaleTypeToAnswerOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScaleType",
                table: "AnswerOption",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Befintliga NPS-rader (skapade innan denna kolumn fanns) landar annars på
            // default-värdet 0 (Standard1To5) - de identifieras här via sitt egna
            // "NPS: {värde}"-beskrivningsmönster, som DataSeeder alltid använt för dem.
            migrationBuilder.Sql(
                "UPDATE \"AnswerOption\" SET \"ScaleType\" = 1 WHERE \"Description\" LIKE 'NPS: %';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScaleType",
                table: "AnswerOption");
        }
    }
}
