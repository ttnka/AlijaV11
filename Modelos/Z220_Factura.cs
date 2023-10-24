using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace DashBoard.Modelos
{
	public class Z220_Factura
	{
        [Key]
        [StringLength(50)]
        public string FacturaId { get; set; } = "";
        [StringLength(50)]
        public string FacturaNum { get; set; } = "";
        [StringLength(50)]
        public string OrgId { get; set; } = "";
        
        public DateTime Fecha { get; set; } = DateTime.Now;
        [StringLength(75)]
        public string Titulo { get; set; } = "";
        public string? Obs { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Importe { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Control { get; set; }
        [StringLength(50)]
        public string Corporativo { get; set; } = "";
        [StringLength(50)]
        public string EmpresaId { get; set; } = "";
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

