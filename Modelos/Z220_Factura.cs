using System;
using System.ComponentModel.DataAnnotations;

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
        [StringLength(50)]
        public string Corporativo { get; set; } = "";
        [StringLength(50)]
        public string EmpresaId { get; set; } = "";
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

