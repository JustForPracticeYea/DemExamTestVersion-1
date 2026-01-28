using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DemExam.Models;

public partial class WorkersMumzhaContext : DbContext
{
    public WorkersMumzhaContext()
    {
    }

    public WorkersMumzhaContext(DbContextOptions<WorkersMumzhaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<Office> Offices { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Worker> Workers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LOX\\SQLEXPRESS;Initial Catalog=Workers_Mumzha;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.ToTable("Auditorium");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AuditoriumName).HasMaxLength(255);
            entity.Property(e => e.Floor).HasMaxLength(255);

            entity.HasOne(d => d.IdOfficeNavigation).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.IdOffice)
                .HasConstraintName("FK_Auditorium_Office");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InventoryNumber).HasMaxLength(255);
            entity.Property(e => e.NameEquipment).HasMaxLength(255);
            entity.Property(e => e.Photo).HasMaxLength(255);
            entity.Property(e => e.TransferToCompanyBalanceDate).HasColumnType("datetime");

            entity.HasOne(d => d.IdAuditoriumNavigation).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.IdAuditorium)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Equipment_Auditorium");
        });

        modelBuilder.Entity<Office>(entity =>
        {
            entity.ToTable("Office");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.ShortName).HasMaxLength(255);

            entity.HasOne(d => d.IdWorkerNavigation).WithMany(p => p.Offices)
                .HasForeignKey(d => d.IdWorker)
                .HasConstraintName("FK_Office_Workers");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Post");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.PostName).HasMaxLength(255);
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Login).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Patronymic).HasMaxLength(255);
            entity.Property(e => e.Surname).HasMaxLength(255);

            entity.HasOne(d => d.IdOfficeNavigation).WithMany(p => p.Workers)
                .HasForeignKey(d => d.IdOffice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Workers_Office");

            entity.HasOne(d => d.IdPostNavigation).WithMany(p => p.Workers)
                .HasForeignKey(d => d.IdPost)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Workers_Post");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
