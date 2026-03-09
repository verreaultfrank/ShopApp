using JobHunter.Domain.Models;
using JobHunter.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobHunter.Infrastructure;

public class JobHunterDbContext : DbContext
{
    public JobHunterDbContext(DbContextOptions<JobHunterDbContext> options) : base(options)
    {
    }

    public DbSet<JobOpportunityEntity> JobLeads { get; set; }
    public DbSet<MaterialEntity> Materials { get; set; }
    public DbSet<JobMaterialLinkEntity> JobMaterialLinks { get; set; }
    public DbSet<StockTypeEntity> StockTypes { get; set; }
    public DbSet<StockItemEntity> StockItems { get; set; }
    public DbSet<PartDesignEntity> PartDesigns { get; set; }
    public DbSet<JobPartDesignLinkEntity> JobPartDesignLinks { get; set; }
    public DbSet<LeadStatusHistoryEntity> LeadStatusHistories { get; set; }
    public DbSet<JobStatusEntity> JobStatuses { get; set; }
    public DbSet<BusinessEntity> Businesses { get; set; }
    public DbSet<ContactEntity> Contacts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── JobOpportunity ───
        modelBuilder.Entity<JobOpportunityEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.MaterialLinks)
                  .WithOne(l => l.Job)
                  .HasForeignKey(l => l.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.PartDesignLinks)
                  .WithOne(l => l.Job)
                  .HasForeignKey(l => l.JobLeadId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.StatusHistories)
                  .WithOne(h => h.Job)
                  .HasForeignKey(h => h.JobLeadId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Business)
                  .WithMany()
                  .HasForeignKey(e => e.BusinessId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PreferredContact)
                  .WithMany()
                  .HasForeignKey(e => e.PreferredContactId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ─── Business ───
        modelBuilder.Entity<BusinessEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Contacts)
                  .WithOne(c => c.Business)
                  .HasForeignKey(c => c.BusinessId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Contact ───
        modelBuilder.Entity<ContactEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // ─── PartDesign ───
        modelBuilder.Entity<PartDesignEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.JobLinks)
                  .WithOne(l => l.PartDesign)
                  .HasForeignKey(l => l.PartDesignId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── JobPartDesignLink ───
        modelBuilder.Entity<JobPartDesignLinkEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.JobLeadId, e.PartDesignId }).IsUnique();
        });

        // ─── LeadStatusHistory ───
        modelBuilder.Entity<LeadStatusHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Status)
                  .WithMany()
                  .HasForeignKey(e => e.JobStatusId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── JobStatuses ───
        modelBuilder.Entity<JobStatusEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ─── Material ───
        modelBuilder.Entity<MaterialEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.JobLinks)
                  .WithOne(l => l.Material)
                  .HasForeignKey(l => l.MaterialId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── JobMaterialLink ───
        modelBuilder.Entity<JobMaterialLinkEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.JobId, e.MaterialId }).IsUnique();
        });

        // ─── StockItem ───
        modelBuilder.Entity<StockItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Material)
                  .WithMany()
                  .HasForeignKey(e => e.MaterialId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.StockType)
                  .WithMany()
                  .HasForeignKey(e => e.StockTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Seed Materials ───
        SeedMaterials(modelBuilder);
    }

    private static void SeedMaterials(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaterialEntity>().HasData(
            // ── Aluminum ──
            new MaterialEntity { Id = 1, Name = "Aluminum 2024-T3", Category = "Aluminum", AmsDesignation = "AMS 4037", UnsDesignation = "A92024", IsoDesignation = "AlCu4Mg1", Form = "Sheet", TemperCondition = "T3" },
            new MaterialEntity { Id = 2, Name = "Aluminum 2024-T351", Category = "Aluminum", AmsDesignation = "AMS 4035", UnsDesignation = "A92024", IsoDesignation = "AlCu4Mg1", Form = "Plate", TemperCondition = "T351" },
            new MaterialEntity { Id = 3, Name = "Aluminum 6061-T6", Category = "Aluminum", AmsDesignation = "AMS 4027", UnsDesignation = "A96061", IsoDesignation = "AlMg1SiCu", Form = "Bar", TemperCondition = "T6" },
            new MaterialEntity { Id = 4, Name = "Aluminum 6061-T651", Category = "Aluminum", AmsDesignation = "AMS 4027", UnsDesignation = "A96061", IsoDesignation = "AlMg1SiCu", Form = "Plate", TemperCondition = "T651" },
            new MaterialEntity { Id = 5, Name = "Aluminum 7075-T6", Category = "Aluminum", AmsDesignation = "AMS 4045", UnsDesignation = "A97075", IsoDesignation = "AlZn5.5MgCu", Form = "Bar", TemperCondition = "T6" },
            new MaterialEntity { Id = 6, Name = "Aluminum 7075-T651", Category = "Aluminum", AmsDesignation = "AMS 4078", UnsDesignation = "A97075", IsoDesignation = "AlZn5.5MgCu", Form = "Plate", TemperCondition = "T651" },
            new MaterialEntity { Id = 7, Name = "Aluminum 7050-T7451", Category = "Aluminum", AmsDesignation = "AMS 4050", UnsDesignation = "A97050", IsoDesignation = "AlZn6CuMgZr", Form = "Plate", TemperCondition = "T7451" },

            // ── Titanium ──
            new MaterialEntity { Id = 8, Name = "Ti-6Al-4V (Grade 5)", Category = "Titanium", AmsDesignation = "AMS 4911", UnsDesignation = "R56400", IsoDesignation = "TiAl6V4", Form = "Bar", TemperCondition = "Annealed" },
            new MaterialEntity { Id = 9, Name = "Ti-6Al-4V ELI (Grade 23)", Category = "Titanium", AmsDesignation = "AMS 4930", UnsDesignation = "R56401", IsoDesignation = "TiAl6V4 ELI", Form = "Bar", TemperCondition = "Annealed" },
            new MaterialEntity { Id = 10, Name = "CP Titanium Grade 2", Category = "Titanium", AmsDesignation = "AMS 4902", UnsDesignation = "R50400", IsoDesignation = "Ti-Gr2", Form = "Sheet", TemperCondition = "Annealed" },
            new MaterialEntity { Id = 11, Name = "Ti-6Al-4V STA", Category = "Titanium", AmsDesignation = "AMS 4928", UnsDesignation = "R56400", IsoDesignation = "TiAl6V4", Form = "Forging", TemperCondition = "STA" },

            // ── Nickel Superalloys ──
            new MaterialEntity { Id = 12, Name = "Inconel 718", Category = "Nickel", AmsDesignation = "AMS 5663", UnsDesignation = "N07718", IsoDesignation = "NiCr19Fe19Nb5Mo3", Form = "Bar", TemperCondition = "Aged" },
            new MaterialEntity { Id = 13, Name = "Inconel 625", Category = "Nickel", AmsDesignation = "AMS 5666", UnsDesignation = "N06625", IsoDesignation = "NiCr22Mo9Nb", Form = "Sheet", TemperCondition = "Annealed" },
            new MaterialEntity { Id = 14, Name = "Waspaloy", Category = "Nickel", AmsDesignation = "AMS 5544", UnsDesignation = "N07001", IsoDesignation = "NiCr20Co13Mo4Ti3Al", Form = "Forging", TemperCondition = "Aged" },
            new MaterialEntity { Id = 15, Name = "Hastelloy X", Category = "Nickel", AmsDesignation = "AMS 5536", UnsDesignation = "N06002", IsoDesignation = "NiCr22Fe18Mo", Form = "Sheet", TemperCondition = "Solution Treated" },
            new MaterialEntity { Id = 16, Name = "Monel K-500", Category = "Nickel", AmsDesignation = "AMS 4676", UnsDesignation = "N05500", IsoDesignation = "NiCu30Al", Form = "Bar", TemperCondition = "Age Hardened" },

            // ── Stainless Steel ──
            new MaterialEntity { Id = 17, Name = "304 Stainless Steel", Category = "Steel", AmsDesignation = "AMS 5513", UnsDesignation = "S30400", IsoDesignation = "X5CrNi18-10", Form = "Sheet", TemperCondition = "Annealed" },
            new MaterialEntity { Id = 18, Name = "316L Stainless Steel", Category = "Steel", AmsDesignation = "AMS 5507", UnsDesignation = "S31603", IsoDesignation = "X2CrNiMo17-12-2", Form = "Bar", TemperCondition = "Annealed" },
            new MaterialEntity { Id = 19, Name = "17-4 PH Stainless", Category = "Steel", AmsDesignation = "AMS 5643", UnsDesignation = "S17400", IsoDesignation = "X5CrNiCuNb16-4", Form = "Bar", TemperCondition = "H900" },
            new MaterialEntity { Id = 20, Name = "15-5 PH Stainless", Category = "Steel", AmsDesignation = "AMS 5659", UnsDesignation = "S15500", IsoDesignation = "X5CrNiCuNb15-5", Form = "Bar", TemperCondition = "H1025" },
            new MaterialEntity { Id = 21, Name = "A286 Iron-Based Super", Category = "Steel", AmsDesignation = "AMS 5731", UnsDesignation = "S66286", IsoDesignation = "X5NiCrTi26-15", Form = "Bar", TemperCondition = "Aged" },

            // ── Alloy Steel ──
            new MaterialEntity { Id = 22, Name = "4340 Alloy Steel", Category = "Steel", AmsDesignation = "AMS 6414", UnsDesignation = "G43400", IsoDesignation = "34CrNiMo6", Form = "Bar", TemperCondition = "Quenched & Tempered" },
            new MaterialEntity { Id = 23, Name = "4130 Alloy Steel", Category = "Steel", AmsDesignation = "AMS 6370", UnsDesignation = "G41300", IsoDesignation = "25CrMo4", Form = "Tube", TemperCondition = "Normalized" },
            new MaterialEntity { Id = 24, Name = "300M Ultra-High Strength", Category = "Steel", AmsDesignation = "AMS 6417", UnsDesignation = "K44220", IsoDesignation = "300M", Form = "Bar", TemperCondition = "Quenched & Tempered" },

            // ── Other ──
            new MaterialEntity { Id = 25, Name = "Copper C110 (ETP)", Category = "Copper", AmsDesignation = null, UnsDesignation = "C11000", IsoDesignation = "Cu-ETP", Form = "Bar", TemperCondition = "Half Hard" },
            new MaterialEntity { Id = 26, Name = "Beryllium Copper C172", Category = "Copper", AmsDesignation = "AMS 4533", UnsDesignation = "C17200", IsoDesignation = "CuBe2", Form = "Bar", TemperCondition = "AT" },
            new MaterialEntity { Id = 27, Name = "Magnesium AZ31B", Category = "Magnesium", AmsDesignation = "AMS 4375", UnsDesignation = "M11311", IsoDesignation = "MgAl3Zn1", Form = "Sheet", TemperCondition = "H24" }
        );
    }
}
