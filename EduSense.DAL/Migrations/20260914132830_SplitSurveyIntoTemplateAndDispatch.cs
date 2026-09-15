using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EduSense.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SplitSurveyIntoTemplateAndDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Respondent_Survey_SurveyId",
                table: "Respondent");

            migrationBuilder.DropIndex(
                name: "IX_Survey_Title_SurveyExpiryDate_OrganisationId",
                table: "Survey");

            migrationBuilder.DropColumn(
                name: "SurveyExpiryDate",
                table: "Survey");

            migrationBuilder.RenameColumn(
                name: "SurveyId",
                table: "Respondent",
                newName: "SurveyDispatchId");

            migrationBuilder.RenameIndex(
                name: "IX_Respondent_SurveyId",
                table: "Respondent",
                newName: "IX_Respondent_SurveyDispatchId");

            migrationBuilder.RenameIndex(
                name: "IX_Respondent_Email_Token_SurveyId",
                table: "Respondent",
                newName: "IX_Respondent_Email_Token_SurveyDispatchId");

            migrationBuilder.CreateTable(
                name: "SurveyDispatch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SurveyId = table.Column<int>(type: "integer", nullable: false),
                    ResponseDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentByUserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyDispatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyDispatch_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Survey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyDispatch_SurveyId",
                table: "SurveyDispatch",
                column: "SurveyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Respondent_SurveyDispatch_SurveyDispatchId",
                table: "Respondent",
                column: "SurveyDispatchId",
                principalTable: "SurveyDispatch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Respondent_SurveyDispatch_SurveyDispatchId",
                table: "Respondent");

            migrationBuilder.DropTable(
                name: "SurveyDispatch");

            migrationBuilder.RenameColumn(
                name: "SurveyDispatchId",
                table: "Respondent",
                newName: "SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_Respondent_SurveyDispatchId",
                table: "Respondent",
                newName: "IX_Respondent_SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_Respondent_Email_Token_SurveyDispatchId",
                table: "Respondent",
                newName: "IX_Respondent_Email_Token_SurveyId");

            migrationBuilder.AddColumn<DateTime>(
                name: "SurveyExpiryDate",
                table: "Survey",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Survey_Title_SurveyExpiryDate_OrganisationId",
                table: "Survey",
                columns: new[] { "Title", "SurveyExpiryDate", "OrganisationId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Respondent_Survey_SurveyId",
                table: "Respondent",
                column: "SurveyId",
                principalTable: "Survey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
