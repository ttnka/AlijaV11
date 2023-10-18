using System;
namespace DashBoard.Modelos
{
	public class AddUser
	{
        public string Mail { get; set; } = "";
        public string Pass { get; set; } = "";
        public string ConfirmPass { get; set; } = "";

        public string Nombre { get; set; } = "";
        public string Paterno { get; set; } = "";
        public string? Materno { get; set; } = "";
        public string OldMail { get; set; } = "";

        public string OrgId { get; set; } = "";
        public string OrgName { get; set; } = "";
        public string Corporativo { get; set; } = "";
        public int Nivel { get; set; } = 1;

        public string? UsuarioId { get; set; } = "";
        public bool Sistema { get; set; } 
    }
}

