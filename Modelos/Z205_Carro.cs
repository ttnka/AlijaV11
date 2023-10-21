using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z205_Carro
	{
        [Key]
        [StringLength(50)]
        public string CarroId { get; set; } = "";
        [StringLength(50)]
        public string FolioId { get; set; } = null!;
        [StringLength(75)]
        public string Tractor { get; set; } = null!;
        [StringLength(50)]
        public string Caja1 { get; set; } = null!;
        public string? Caja2 { get; set; }
        public string? CajaOtros { get; set; }
        public string? Pedimento { get; set; }
        public string? Obs { get; set; }
        
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

