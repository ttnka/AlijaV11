using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z222_FactDet
	{
        [Key]
        [StringLength(50)]
        public string FactDetId { get; set; } = "";
        [StringLength(50)]
        public string FacturaId { get; set; } = "";
        [StringLength(50)]
        public string FolioId { get; set; } = "";
        public string? Obs { get; set; }
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

