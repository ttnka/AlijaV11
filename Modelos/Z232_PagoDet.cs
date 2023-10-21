using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DashBoard.Modelos
{
	public class Z232_PagoDet
	{
        [Key]
        [StringLength(50)]
        public string PagDetId { get; set; } = "";
        [StringLength(50)]
        public string PagoId { get; set; } = "";
        [StringLength(50)]
        public string FacturaId { get; set; } = "";
        [Column(TypeName = "decimal(18,2)")]
        public decimal ImporteDet { get; set; }
        public string? Obs { get; set; }
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

