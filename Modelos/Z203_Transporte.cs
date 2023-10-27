using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z203_Transporte
	{
        [Key]
        [StringLength(50)]
        public string TransporteId { get; set; } = "";
        [StringLength(50)]
        public string FolioId { get; set; } = "";
        [StringLength(75)]
        public string Transportista { get; set; } = "";
        [StringLength(75)]
        public string Chofer { get; set; } = "";
        [StringLength(50)]
        public string CartaPorte { get; set; } = "";
        public string? Identificacion { get; set; }
        
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

