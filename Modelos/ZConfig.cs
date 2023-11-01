using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class ZConfig
	{
        [Key]
        [StringLength(50)]
        public string ConfigId { get; set; } = "";
        [StringLength(50)]
        public string Usuario { get; set; } = "";
        [StringLength(25)]
        public string Grupo { get; set; } = "";
        [StringLength(25)]
        public string Tipo { get; set; } = "";
        [StringLength(50)]
        public string Titulo { get; set; } = "";

        public string? Txt { get; set; } = "";
        public int Entero { get; set; } = 0;
        public decimal Decimal { get; set; } = 0;
        public DateTime Fecha1 { get; set; }
        public DateTime Fecha2 { get; set; }
        public bool SiNo { get; set; } = true;

        public int Estado { get; set; } = 0;
        public bool Status { get; set; } = true;
    }
}

