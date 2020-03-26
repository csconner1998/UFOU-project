using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UFOU.Models;

namespace UFOU.Data
{
    public class UFOContext : DbContext
    {
        public UFOContext(DbContextOptions<UFOContext> options)
            : base(options)
        {
        }

        public DbSet<Report> Reports { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<BarGraph> BarGraphs { get; set; }
        public DbSet<Favorite> Favorites { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ReportConfiguration());
        }

        /// <summary>
        /// Fluent API configuration for Reports table
        /// </summary>
        public class ReportConfiguration : IEntityTypeConfiguration<Report>
        {
            public void Configure(EntityTypeBuilder<Report> builder)
            {
                builder.HasKey(r => r.ReportId);

                builder.Property(r => r.Approved)
                    .HasDefaultValue(false)
                    .ValueGeneratedOnAdd();

                builder.Property(r => r.DateSubmitted)
                    .HasDefaultValueSql("getdate()")
                    .ValueGeneratedOnAdd();

                builder.ToTable("Reports");

            }
        }

    }
}
