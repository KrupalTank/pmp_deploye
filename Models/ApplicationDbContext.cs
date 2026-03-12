using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PlacementMentorshipPortal.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Audit> Audits { get; set; }
    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Coordinator> Coordinators { get; set; }

    public virtual DbSet<Description> Descriptions { get; set; }

    public virtual DbSet<Resource> Resources { get; set; }

    public virtual DbSet<Rounddetail> Rounddetails { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Studentsplaced> Studentsplaceds { get; set; }

    public virtual DbSet<StudentCount> StudentCounts { get; set; }

    public virtual DbSet<Year> Years { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Bid).HasName("branch_pkey");

            entity.ToTable("branch");

            entity.HasIndex(e => e.Bname, "branch_bname_key").IsUnique();

            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Bname)
                .HasMaxLength(10)
                .HasColumnName("bname");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Cid).HasName("company_pkey");

            entity.ToTable("company");

            entity.Property(e => e.Cid).HasColumnName("cid");
            entity.Property(e => e.Cname)
                .HasMaxLength(100)
                .HasColumnName("cname");
            entity.Property(e => e.Logo).HasColumnName("logo");
            entity.Property(e => e.Tid).HasColumnName("tid");

            entity.HasOne(d => d.TidNavigation).WithMany(p => p.Companies)
                .HasForeignKey(d => d.Tid)
                .HasConstraintName("company_tid_fkey");
        });

        modelBuilder.Entity<Coordinator>(entity =>
        {
            entity.HasKey(e => e.Tid).HasName("coordinators_pkey");

            entity.ToTable("coordinators");

            entity.HasIndex(e => e.Uid, "coordinators_uid_key").IsUnique();

            entity.Property(e => e.Tid).HasColumnName("tid");
            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasColumnName("active");
            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Contact)
                .HasMaxLength(50)
                .HasColumnName("contact");
            entity.Property(e => e.Pwd)
                .HasMaxLength(255)
                .HasColumnName("pwd");
            entity.Property(e => e.Tname)
                .HasMaxLength(100)
                .HasColumnName("tname");
            entity.Property(e => e.Uid)
                .HasMaxLength(50)
                .HasColumnName("uid");
            entity.Property(e => e.Yid).HasColumnName("yid");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.Coordinators)
                .HasForeignKey(d => d.Bid)
                .HasConstraintName("coordinators_bid_fkey");

            entity.HasOne(d => d.YidNavigation).WithMany(p => p.Coordinators)
                .HasForeignKey(d => d.Yid)
                .HasConstraintName("coordinators_yid_fkey");
        });

        modelBuilder.Entity<Description>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("description_pkey");

            entity.ToTable("description");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cid).HasColumnName("cid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Dtext).HasColumnName("dtext");
            entity.Property(e => e.Tid).HasColumnName("tid");

            entity.HasOne(d => d.CidNavigation).WithMany(p => p.Descriptions)
                .HasForeignKey(d => d.Cid)
                .HasConstraintName("description_cid_fkey");

            entity.HasOne(d => d.TidNavigation).WithMany(p => p.Descriptions)
                .HasForeignKey(d => d.Tid)
                .HasConstraintName("description_tid_fkey");
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("resources_pkey");

            entity.ToTable("resources");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Rlink).HasColumnName("rlink");
            entity.Property(e => e.Tid).HasColumnName("tid");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.Resources)
                .HasForeignKey(d => d.Bid)
                .HasConstraintName("resources_bid_fkey");

            entity.HasOne(d => d.TidNavigation).WithMany(p => p.Resources)
                .HasForeignKey(d => d.Tid)
                .HasConstraintName("resources_tid_fkey");
        });

        modelBuilder.Entity<Rounddetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rounddetails_pkey");

            entity.ToTable("rounddetails");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cid).HasColumnName("cid");
            entity.Property(e => e.Dtext).HasColumnName("dtext");
            entity.Property(e => e.Tid).HasColumnName("tid");

            entity.HasOne(d => d.CidNavigation).WithMany(p => p.Rounddetails)
                .HasForeignKey(d => d.Cid)
                .HasConstraintName("rounddetails_cid_fkey");

            entity.HasOne(d => d.TidNavigation).WithMany(p => p.Rounddetails)
                .HasForeignKey(d => d.Tid)
                .HasConstraintName("rounddetails_tid_fkey");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sessions_pkey");

            entity.ToTable("sessions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Detail).HasColumnName("detail");
            entity.Property(e => e.Link).HasColumnName("link");
            entity.Property(e => e.Tid).HasColumnName("tid");
            entity.Property(e => e.Time).HasColumnName("time");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.Bid)
                .HasConstraintName("sessions_bid_fkey");

            entity.HasOne(d => d.TidNavigation).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.Tid)
                .HasConstraintName("sessions_tid_fkey");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("students_pkey");

            entity.ToTable("students");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bname)
                .HasMaxLength(10)
                .HasColumnName("bname");
            entity.Property(e => e.Entryyear).HasColumnName("entryyear");
            entity.Property(e => e.Mail)
                .HasMaxLength(100)
                .HasColumnName("mail");
        });

        modelBuilder.Entity<Studentsplaced>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("studentsplaced_pkey");

            entity.ToTable("studentsplaced");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Cid).HasColumnName("cid");
            entity.Property(e => e.Contact)
                .HasMaxLength(50)
                .HasColumnName("contact");
            entity.Property(e => e.Package)
                .HasPrecision(10, 2)
                .HasColumnName("package");
            entity.Property(e => e.Sname)
                .HasMaxLength(100)
                .HasColumnName("sname");
            entity.Property(e => e.Tid).HasColumnName("tid");
            entity.Property(e => e.Yid).HasColumnName("yid");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.Studentsplaceds)
                .HasForeignKey(d => d.Bid)
                .HasConstraintName("studentsplaced_bid_fkey");

            entity.HasOne(d => d.CidNavigation).WithMany(p => p.Studentsplaceds)
                .HasForeignKey(d => d.Cid)
                .HasConstraintName("studentsplaced_cid_fkey");

            entity.HasOne(d => d.TidNavigation).WithMany(p => p.Studentsplaceds)
                .HasForeignKey(d => d.Tid)
                .HasConstraintName("studentsplaced_tid_fkey");

            entity.HasOne(d => d.YidNavigation).WithMany(p => p.Studentsplaceds)
                .HasForeignKey(d => d.Yid)
                .HasConstraintName("studentsplaced_yid_fkey");
        });

        modelBuilder.Entity<StudentCount>(entity =>
        {
            // Define the Primary Key matching your SERIAL PK
            entity.HasKey(e => e.Id).HasName("studentcount_pkey");

            // Map to the exact table name in PostgreSQL
            entity.ToTable("studentcount");

            // Define Unique Constraint to prevent duplicate year-branch entries
            entity.HasIndex(e => new { e.Yid, e.Bid }, "uq_year_branch").IsUnique();

            // Map Properties to Column Names
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Yid).HasColumnName("yid");
            entity.Property(e => e.Bid).HasColumnName("bid");
            entity.Property(e => e.Count)
                .HasDefaultValue(0)
                .HasColumnName("count");

            // Relationship: StudentCount belongs to one Year
            entity.HasOne(d => d.YearNavigation)
                .WithMany(p => p.StudentCounts) // You may need to add this ICollection to Year.cs
                .HasForeignKey(d => d.Yid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_year");

            // Relationship: StudentCount belongs to one Branch
            entity.HasOne(d => d.BranchNavigation)
                .WithMany(p => p.StudentCounts) // You may need to add this ICollection to Branch.cs
                .HasForeignKey(d => d.Bid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_branch");
        });

        modelBuilder.Entity<Year>(entity =>
        {
            entity.HasKey(e => e.Yid).HasName("year_pkey");

            entity.ToTable("year");

            entity.HasIndex(e => e.Year1, "year_year_key").IsUnique();

            entity.Property(e => e.Yid).HasColumnName("yid");
            entity.Property(e => e.Year1).HasColumnName("year");
        });

        modelBuilder.Entity<Audit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("audit_pkey");

            entity.ToTable("audit");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Tid).HasColumnName("tid");

            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasColumnName("action");

            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");

            entity.Property(e => e.Detail).HasColumnName("detail");

            // Configure the default timestamp behavior
            entity.Property(e => e.Time)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("time");

            entity.HasOne(d => d.TidNavigation)
                .WithMany(p => p.Audits)
                .HasForeignKey(d => d.Tid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("audit_tid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
