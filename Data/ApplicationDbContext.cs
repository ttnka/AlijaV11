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

    public DbSet<ZConfig> Configuraciones { get; set; }
    public DbSet<Z100_Org> Organizaciones { get; set; }
    //public DbSet<Z104_Domicilio> Domicilios { get; set; }
    public DbSet<Z110_User> Usuarios { get; set; }
    public DbSet<Z170_File> Archivos { get; set; }
    public DbSet<Z180_EmpActiva> EmpresaActiva { get; set; }
    public DbSet<Z190_Bitacora> Bitacora { get; set; }
    public DbSet<Z192_Logs> LogsBitacora { get; set; }
    public DbSet<Z200_Folio> Folios { get; set;}
    public DbSet<Z201_FolioPrint> FolioPrints { get; set; }
    /*
    public DbSet<Z203_Transporte> Transportistas { get; set;}
    public DbSet<Z204_Empleado> Empleados { get; set;}
    public DbSet<Z205_Carro> Carros { get; set;}
    public DbSet<Z209_Campos> Campos { get; set; }
    public DbSet<Z210_Concepto> Concpetos { get; set;}
    */
    public DbSet<Z220_Factura> Facturas { get; set;}
    public DbSet<Z222_FactDet> FacturasDet { get; set;}
    public DbSet<Z230_Pago> Pagos { get; set; }
    public DbSet<Z232_PagoDet> PagosDet { get; set; }
    public DbSet<Z280_Producto> Productos { get; set;}
}

