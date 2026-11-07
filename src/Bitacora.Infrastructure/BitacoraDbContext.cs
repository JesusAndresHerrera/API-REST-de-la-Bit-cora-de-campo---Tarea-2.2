using Bitacora.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bitacora.Infrastructure;

public class BitacoraDbContext : DbContext
{
    public BitacoraDbContext(DbContextOptions<BitacoraDbContext> options)
        : base(options)
    {
    }

    public DbSet<Registro> Registros => Set<Registro>();

    public DbSet<Fotografia> Fotografias => Set<Fotografia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var registro = modelBuilder.Entity<Registro>();
        registro.ToTable("Registros", table =>
        {
            table.HasCheckConstraint(
                "CK_Registros_Titulo_Longitud",
                "length(Titulo) BETWEEN 3 AND 100");
            table.HasCheckConstraint(
                "CK_Registros_Descripcion_Longitud",
                "length(Descripcion) BETWEEN 1 AND 500");
        });
        registro.HasKey(r => r.Id);
        registro.Property(r => r.Id).ValueGeneratedOnAdd();
        registro.Property(r => r.Titulo).IsRequired().HasMaxLength(100);
        registro.Property(r => r.Descripcion).IsRequired().HasMaxLength(500);
        registro.Property(r => r.FechaCreacion)
            .HasColumnType("TEXT")
            .HasConversion(
                fecha => fecha,
                fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc))
            .IsRequired();

        var fotografia = modelBuilder.Entity<Fotografia>();
        fotografia.ToTable("Fotografias");
        fotografia.HasKey(f => f.RegistroId);
        fotografia.Property(f => f.RegistroId).ValueGeneratedNever();
        fotografia.Property(f => f.FotoBase64).IsRequired();

        registro.HasOne(r => r.Fotografia)
            .WithOne()
            .HasForeignKey<Fotografia>(f => f.RegistroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
