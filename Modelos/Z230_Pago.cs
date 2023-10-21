using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;


namespace DashBoard.Modelos
{
	public class Z230_Pago
	{
        [Key]
        [StringLength(50)]
        public string PagoId { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string PagoFolio { get; set; } = "";
        [StringLength(50)]
        public string OrgId { get; set; } = "";
        
        public string? Obs { get; set; }
        public string? Tipo { get; set; }
        public string? Ticket { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Importe { get; set; }
        [StringLength(50)]
        public string Corporativo { get; set; } = "";
        [StringLength(50)]
        public string EmpresaId { get; set; } = "";
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

