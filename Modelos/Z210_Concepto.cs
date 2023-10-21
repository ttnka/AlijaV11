using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace DashBoard.Modelos
{
	public class Z210_Concepto
    { 
        [Key]
        [StringLength(50)]
        public string ConceptoId { get; set; } = "";
        [StringLength(50)]
        public string FolioId { get; set; } = "";
        public int Renglon { get; set; } = 1;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cantidad { get; set; } = 0;
        [StringLength(50)]
        public string Producto { get; set; } = "";
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; } = 0;
        public int Descuento { get; set; } = 0;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Importe { get; set; } = 0;
        public string? Obs { get; set; }
        
        
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;

    }
}

