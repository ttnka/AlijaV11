using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z209_Campos
	{
        [Key]
        [StringLength(50)]
        public string CampoId { get; set; } = "";
        [StringLength(50)]
        public string FolioId { get; set; } = ""!;
        [StringLength(75)]
        public string? AgenteAduanal { get; set; } = "";
        [StringLength(75)]
        public string? Pedimento { get; set; } = "";
        [StringLength(75)]
        public string? Factura { get; set; } = "";
        [StringLength(50)]
        public string? Mercancia { get; set; } = "";
        [StringLength(20)]
        public string Tractor { get; set; } = "";
        [StringLength(20)]
        public string Caja1 { get; set; } = "";
        [StringLength(20)]
        public string? Caja2 { get; set; } = "";
        [StringLength(20)]
        public string? Placas { get; set; } = "";
        [StringLength(20)]
        public string TractorTipo { get; set; } = "";
        [StringLength(75)]
        public string Transportista { get; set; } = "";
        [StringLength(25)]
        public string Maniobra { get; set; } = "";
        [StringLength(75)]
        public string Despachador { get; set; } = "";
        [StringLength(75)]
        public string CartaPorte { get; set; } = "";
        [StringLength(75)]
        public string SelloRemovido { get; set; } = "";
        [StringLength(75)]
        public string SelloColocado { get; set; } = "";
        [StringLength(75)]
        public string Chofer { get; set; } = "";
        [StringLength(50)]
        public string Identificacion { get; set; } = "";
        [StringLength(75)]
        public string Cuadrilla { get; set; } = "";
        public string Obs { get; set; } = "";
        public string Mails { get; set; } = "";
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

