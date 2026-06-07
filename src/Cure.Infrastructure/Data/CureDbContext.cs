using System.Reflection;
using Cure.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Cure.Domain.Entities.Identity;

namespace Cure.Infrastructure.Data;

public sealed class CureDbContext : IdentityDbContext<ApplicationUser>
{
    public CureDbContext(DbContextOptions<CureDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Nurse> Nurses => Set<Nurse>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<MedicalHistory> MedicalHistories => Set<MedicalHistory>();

    public DbSet<ClinicalNote> ClinicalNotes => Set<ClinicalNote>();

    public DbSet<PatientFile> PatientFiles => Set<PatientFile>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(CureDbContext).Assembly);
    }
}
