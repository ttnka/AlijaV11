using System;
using System.ComponentModel.DataAnnotations;

namespace DashBoard.Modelos
{
	public class Z100_Org
	{
        [Key]
        [StringLength(50)]
        public string OrgId { get; set; } = Guid.NewGuid().ToString();
        [StringLength(15)]
        public string Rfc { get; set; } = "";
        [StringLength(25)]
        public string Comercial { get; set; } = "";
        [StringLength(75)]
        public string? RazonSocial { get; set; } = "";
        public bool Moral { get; set; } = true;
        
        public string? NumCliente { get; set; } = "";
        [StringLength(15)]
        public string Tipo { get; set; } = "Cliente";
        [StringLength(50)]
        public string Corporativo { get; set; } = "All";
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
    }
}

