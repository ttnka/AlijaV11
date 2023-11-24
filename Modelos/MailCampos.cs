using System;
namespace DashBoard.Modelos
{
	public class MailCampos
	{
        public List<string> ParaNombre { get; set; } = new List<string>();
        public List<string> ParaEmail { get; set; } = new List<string>();
        public string Titulo { get; set; } = "";
        public string Cuerpo { get; set; } = "";

        public string UserId { get; set; } = "";
        public string OrgId { get; set; } = "";
        public string Corporativo { get; set; } = "";


        public string SenderName { get; set; } = null!;
        public string SenderEmail { get; set; } = null!;

        public string Server { get; set; } = null!;
        public int Port { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;

        public MailCampos PoblarMail(List<string> paraNombre, List<string> paraEmail, string titulo, string cuerpo,
            string userId, string orgId, string corp, string senderName,
            string senderEMail, string server, int port, string userName, string password)
        {
            this.ParaEmail = paraEmail;
            this.Titulo = titulo;
            this.Cuerpo = cuerpo;
            this.ParaNombre = paraNombre;
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

