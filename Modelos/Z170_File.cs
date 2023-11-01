using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z170_File
	{
        [Key]
        [StringLength(50)]
        public string FileId { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;

        [StringLength(25)]
        public string Tipo { get; set; } = "";
        public string Folder { get; set; } = "";
        public string Archivo { get; set; } = "";

        [StringLength(50)]
        public string FolioId { get; set; } = "";
        [StringLength(50)]
        public string FacturaId { get; set; } = "";
        [StringLength(50)]
        public string OrgId { get; set; } = "";
        [StringLength(25)]
        public string Grupo { get; set; } = "All";
        [StringLength(50)]
        public string EmpresaActiva { get; set; } = "";

        [StringLength(75)]
        public string Titulo { get; set; } = "";
        

        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

