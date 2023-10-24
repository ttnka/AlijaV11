using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace DashBoard.Modelos
{
	public class Z200_Folio
    {
        [Key]
        [StringLength(50)]
        public string FolioId { get; set; } = "";
        
        public int FolioNum { get; set; } 
        [StringLength(50)]
        public string OrgId { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Importe { get; set; } = 0;
        [StringLength(50)]
        public string Titulo { get; set; } = "";
        public string? Obs { get; set; }
        [StringLength(50)]
        public string Corporativo { get; set; } = "";
        [StringLength(50)]
        public string EmpresaId { get; set; } = "";
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;

        public string FactFolio => $"{FolioNum} {Titulo}";
    
	}
}

