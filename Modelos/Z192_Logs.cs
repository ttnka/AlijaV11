using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z192_Logs
	{
        [Key]
        [StringLength(50)]
        public string BitacoraId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Fecha { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string UserId { get; set; } = "";
        public string Desc { get; set; } = "";
        public bool Sistema { get; set; } = false;
        [StringLength(50)]
        public string OrgId { get; set; } = "";
        [StringLength(50)]
        public string Corporativo { get; set; } = "All";
        [StringLength(50)]
        public string EmpresaId { get; set; } = "Tbd";
    }
}

