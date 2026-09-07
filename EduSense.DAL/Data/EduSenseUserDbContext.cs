using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EduSense.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.DAL.Data
{
    public class EduSenseUserDbContext : IdentityDbContext<ApplicationUser>
    {
        public EduSenseUserDbContext(DbContextOptions<EduSenseUserDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // builder.Entity<ApplicationUser>().ToTable("Users");

            builder.Entity<RefreshTokenModel>()
                .HasIndex(rt => rt.Token)
                .IsUnique();
        }

        public DbSet<RefreshTokenModel> RefreshTokens => Set<RefreshTokenModel>();
    }
}
