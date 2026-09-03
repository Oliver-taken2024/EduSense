using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EduSense.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnswerOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerOption", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organisation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganisationId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationUser_Organisation_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Survey",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    SurveyExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrganisationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Survey", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Survey_Organisation_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionAnswerOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    AnswerOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionAnswerOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionAnswerOption_AnswerOption_AnswerOptionId",
                        column: x => x.AnswerOptionId,
                        principalTable: "AnswerOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionAnswerOption_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Respondent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    SurveyId = table.Column<int>(type: "integer", nullable: false),
                    TokenIsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    TokenUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Respondent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Respondent_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Survey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SurveyQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SurveyId = table.Column<int>(type: "integer", nullable: false),
                    QuestionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyQuestion_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurveyQuestion_Survey_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Survey",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Response",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RespondentId = table.Column<int>(type: "integer", nullable: false),
                    SurveyQuestionId = table.Column<int>(type: "integer", nullable: false),
                    QuestionAnswerOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Response", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Response_QuestionAnswerOption_QuestionAnswerOptionId",
                        column: x => x.QuestionAnswerOptionId,
                        principalTable: "QuestionAnswerOption",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Response_Respondent_RespondentId",
                        column: x => x.RespondentId,
                        principalTable: "Respondent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Response_SurveyQuestion_SurveyQuestionId",
                        column: x => x.SurveyQuestionId,
                        principalTable: "SurveyQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationUser_OrganisationId_UserId",
                table: "OrganisationUser",
                columns: new[] { "OrganisationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAnswerOption_AnswerOptionId",
                table: "QuestionAnswerOption",
                column: "AnswerOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionAnswerOption_QuestionId_AnswerOptionId",
                table: "QuestionAnswerOption",
                columns: new[] { "QuestionId", "AnswerOptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Respondent_Email_Token_SurveyId",
                table: "Respondent",
                columns: new[] { "Email", "Token", "SurveyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Respondent_SurveyId",
                table: "Respondent",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_Response_QuestionAnswerOptionId",
                table: "Response",
                column: "QuestionAnswerOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Response_RespondentId",
                table: "Response",
                column: "RespondentId");

            migrationBuilder.CreateIndex(
                name: "IX_Response_SurveyQuestionId",
                table: "Response",
                column: "SurveyQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Survey_OrganisationId",
                table: "Survey",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_Survey_Title_SurveyExpiryDate_OrganisationId",
                table: "Survey",
                columns: new[] { "Title", "SurveyExpiryDate", "OrganisationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestion_QuestionId",
                table: "SurveyQuestion",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestion_SurveyId_QuestionId",
                table: "SurveyQuestion",
                columns: new[] { "SurveyId", "QuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganisationUser");

            migrationBuilder.DropTable(
                name: "Response");

            migrationBuilder.DropTable(
                name: "QuestionAnswerOption");

            migrationBuilder.DropTable(
                name: "Respondent");

            migrationBuilder.DropTable(
                name: "SurveyQuestion");

            migrationBuilder.DropTable(
                name: "AnswerOption");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "Survey");

            migrationBuilder.DropTable(
                name: "Organisation");
        }
    }
}
