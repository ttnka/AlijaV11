using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z204_Empleado
	{
        [Key]
        [StringLength(50)]
        public string EmpleadoId { get; set; } = "";
        [StringLength(50)]
        public string FolioId { get; set; } = "";
        [StringLength(75)]
        public string Nombre { get; set; } = "";
        public string? Obs { get; set; }

        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

