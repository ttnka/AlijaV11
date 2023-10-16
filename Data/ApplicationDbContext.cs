using DashBoard.Modelos;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DashBoard.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }

    public DbSet<Z100_Org> Organizaciones { get; set; }
    
    //public DbSet<Z104_Domicilio> Domicilios { get; set; }
    public DbSet<Z110_User> Usuarios { get; set; }
    //public DbSet<Z180_File> Archivos { get; set; }
    public DbSet<Z190_Bitacora> Bitacora { get; set; }
    public DbSet<Z192_Logs> LogsBitacora { get; set; }
}

