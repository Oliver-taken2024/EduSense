using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EduSense.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EduSense.DAL.Data
{
    // IDataProtectionKeyContext - gör att den delade nyckelringen för invite-/
    // återställningstokens kan lagras i samma (delade) databas istället för
    // lokalt per maskin. Utan detta kan en token som skapats på en maskin inte
    // valideras av API:et på en annan.
    public class EduSenseUserDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
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

        public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys => Set<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey>();
    }
}
