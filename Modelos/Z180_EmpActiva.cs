using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z180_EmpActiva
	{
        [Key]
        [StringLength(50)]
        public string EmpActId { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string OrgId { get; set; } = "";
        [StringLength(50)]
        public string UserId { get; set; } = "";
        public int Estado { get; set; } = 0;
        public bool Estatus { get; set; } = true;
    }
}

