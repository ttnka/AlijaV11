using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z200_Folio
    {
        [Key]
        [StringLength(50)]
        public string FolioId { get; set; } = "";
        [StringLength(10)]
        public string FolioNum { get; set; } = "";
        [StringLength(50)]
        public string OrgId { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
        [StringLength(50)]
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

