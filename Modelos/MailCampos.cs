using System;
namespace DashBoard.Modelos
{
	public class MailCampos
	{
        public string Para { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Cuerpo { get; set; } = "";

        public string Nombre { get; set; } = "";
        public string UserId { get; set; } = "";
        public string OrgId { get; set; } = "";
        public string Corporativo { get; set; } = "";


        public string SenderName { get; set; } = null!;
        public string SenderEmail { get; set; } = null!;

        public string Server { get; set; } = null!;
        public int Port { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;

        public MailCampos PoblarMail(string para, string titulo, string cuerpo,
            string nombre, string userId, string orgId, string corp, string senderName,
            string senderEMail, string server, int port, string userName, string password)
        {
            this.Para = para;
            this.Titulo = titulo;
            this.Cuerpo = cuerpo;
            this.Nombre = nombre;
            this.UserId = userId;
            this.OrgId = orgId;
            this.Corporativo = corp; 
            this.SenderName = senderName;
            this.SenderEmail = senderEMail;
            this.Server = server;
            this.Port = port;
            this.UserName = userName;
            this.Password = password;
            return this;
        }
    }
}

