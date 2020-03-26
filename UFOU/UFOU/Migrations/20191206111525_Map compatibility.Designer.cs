﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UFOU.Data;

namespace UFOU.Migrations
{
    [DbContext(typeof(UFOContext))]
    [Migration("20191206111525_Map compatibility")]
    partial class Mapcompatibility
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("UFOU.Models.BarGraph", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Location");

                    b.Property<int>("Quantity");

                    b.Property<int>("Shape");

                    b.HasKey("ID");

                    b.ToTable("BarGraphs");
                });

            modelBuilder.Entity("UFOU.Models.Location", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("MostCommonShape");

                    b.Property<string>("Name");

                    b.Property<int>("Sightings");

                    b.HasKey("ID");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("UFOU.Models.Report", b =>
                {
                    b.Property<int>("ReportId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Approved")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<DateTime>("DateOccurred");

                    b.Property<DateTime?>("DatePosted");

                    b.Property<DateTime>("DateSubmitted")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("getdate()");

                    b.Property<string>("Description")
                        .HasMaxLength(5000);

                    b.Property<string>("Duration");

                    b.Property<string>("Location");

                    b.Property<int>("Shape");

                    b.HasKey("ReportId");

                    b.ToTable("Reports");
                });
#pragma warning restore 612, 618
        }
    }
}
