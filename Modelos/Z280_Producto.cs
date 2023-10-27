using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DashBoard.Modelos
{
	public class Z280_Producto
	{
        [Key]
        [StringLength(50)]
        public string ProductoId { get; set; } = "";
        [StringLength(20)]
        public string Grupo { get; set; } = "";
        public string? Tipo { get; set; }
        [StringLength(10)]
        public string Clave { get; set; } = "";
        [StringLength(75)]
        public string Titulo { get; set; } = "";
        [Column(TypeName = "decimal(18,2)")]    
        public decimal Precio { get; set; }
        public string? Obs { get; set; }
        [StringLength(50)]
        public string Corporativo { get; set; } = "";
        
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
        public string ClaveNombre => $"{Clave} {Titulo}";
        public string CNP => $"{Clave} {Titulo} {Precio}";
    }
}

