using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z110_User
	{
        [Key]
        [StringLength(50)]
        public string UserId { get; set; } = "";
        [StringLength(25)]
        public string Nombre { get; set; } = "";
        [StringLength(25)]
        public string Paterno { get; set; } = "";
        [StringLength(25)]
        public string? Materno { get; set; }
        public int Nivel { get; set; } = 1;
        [StringLength(50)]
        public string OrgId { get; set; } = "";
        [StringLength(75)]
        public string? OldEmail { get; set; }
        [StringLength(50)]
        public string Corporativo { get; set; } = "";
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
        public string Completo => Nombre + " " + Paterno + " " + Materno;
    }
}

