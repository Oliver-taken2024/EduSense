using EduSense.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.DAL.Data
{
    public class EduSenseDbContext : DbContext
    {
        public EduSenseDbContext(DbContextOptions<EduSenseDbContext> options) : base(options)
        {
        }

        public DbSet<OrganisationModel> Organisations => Set<OrganisationModel>();
        public DbSet<OrganisationUserModel> OrganisationUsers => Set<OrganisationUserModel>();
        public DbSet<QuestionModel> Questions => Set<QuestionModel>();
        public DbSet<SurveyModel> Surveys => Set<SurveyModel>();
        public DbSet<SurveyQuestionModel> SurveyQuestions => Set<SurveyQuestionModel>();
        public DbSet<AnswerOptionModel> AnswerOptions => Set<AnswerOptionModel>();
        public DbSet<QuestionAnswerOptionModel> QuestionAnswerOptions => Set<QuestionAnswerOptionModel>();
        public DbSet<RespondentModel> Respondents => Set<RespondentModel>();
        public DbSet<ResponseModel> Responses => Set<ResponseModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrganisationModel>(entity =>
            {
                entity.ToTable("Organisation");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired();

                entity.HasMany(x => x.OrganisationUsers)
                    .WithOne(x => x.Organisation)
                    .HasForeignKey(x => x.OrganisationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Surveys)
                    .WithOne(x => x.Organisation)
                    .HasForeignKey(x => x.OrganisationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrganisationUserModel>(entity =>
            {
                entity.ToTable("OrganisationUser");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.UserId)
                    .IsRequired();

                // En användare får bara förekomma en gång per organisation:
                entity.HasIndex(x => new { x.OrganisationId, x.UserId })
                    .IsUnique();
            });

            modelBuilder.Entity<QuestionModel>(entity =>
            {
                entity.ToTable("Question");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Text)
                    .IsRequired();

                entity.Property(x => x.CreatedByUserId)
                    .IsRequired();

                entity.HasMany(x => x.SurveyQuestions)
                    .WithOne(x => x.Question)
                    .HasForeignKey(x => x.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.QuestionAnswerOptions)
                    .WithOne(x => x.Question)
                    .HasForeignKey(x => x.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SurveyModel>(entity =>
            {
                entity.ToTable("Survey");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .IsRequired();

                entity.Property(x => x.CreatedByUserId)
                    .IsRequired();

                entity.Property(x => x.SurveyExpiryDate)
                    .IsRequired();

                entity.HasIndex(x => new { x.Title, x.SurveyExpiryDate, x.OrganisationId })
                    .IsUnique();

                entity.HasMany(x => x.SurveyQuestions)
                    .WithOne(x => x.Survey)
                    .HasForeignKey(x => x.SurveyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Respondents)
                    .WithOne(x => x.Survey)
                    .HasForeignKey(x => x.SurveyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SurveyQuestionModel>(entity =>
            {
                entity.ToTable("SurveyQuestion");
                entity.HasKey(x => x.Id);

                // Samma fråga ska inte kunna läggas till två gånger i samma survey:
                entity.HasIndex(x => new { x.SurveyId, x.QuestionId })
                    .IsUnique();

                entity.HasMany(x => x.Responses)
                    .WithOne(x => x.SurveyQuestion)
                    .HasForeignKey(x => x.SurveyQuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AnswerOptionModel>(entity =>
            {
                entity.ToTable("AnswerOption");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Description)
                    .IsRequired();

                entity.Property(x => x.Value)
                    .IsRequired();

                entity.HasMany(x => x.QuestionAnswerOptions)
                    .WithOne(x => x.AnswerOption)
                    .HasForeignKey(x => x.AnswerOptionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<QuestionAnswerOptionModel>(entity =>
            {
                entity.ToTable("QuestionAnswerOption");
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => new { x.QuestionId, x.AnswerOptionId })
                    .IsUnique();

                entity.HasMany(x => x.Responses)
                    .WithOne(x => x.QuestionAnswerOption)
                    .HasForeignKey(x => x.QuestionAnswerOptionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RespondentModel>(entity =>
            {
                entity.ToTable("Respondent");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Email)
                    .IsRequired();

                entity.Property(x => x.Token)
                    .IsRequired();

                entity.Property(x => x.TokenIsUsed)
                    .IsRequired();

                entity.HasIndex(x => new { x.Email, x.Token, x.SurveyId })
                    .IsUnique();

                entity.HasMany(x => x.Responses)
                    .WithOne(x => x.Respondent)
                    .HasForeignKey(x => x.RespondentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ResponseModel>(entity =>
            {
                entity.ToTable("Response");
                entity.HasKey(x => x.Id);
            });
        }
    }
}
