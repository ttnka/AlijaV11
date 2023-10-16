
using System;
using static System.Net.WebRequestMethods;

namespace DashBoard.Modelos
{
	public class Constantes
	{
        public const string ElDominio = "Datason.com.mx";
        // Mail 01

        public const string DeNombreMail01 = "WebMaster";
        public const string DeMail01 = "info@datason.com.mx";
        public const string ServerMail01 = "datason.com.mx";
        public const int PortMail01 = 587;
        public const int Nivel01 = 1;
        public const string UserNameMail01 = "info@datason.com.mx";
        public const string PasswordMail01 = "2468022Ih.";

        // Mail 02
        public const string DeNombreMail02 = "WebMaster";
        public const string DeMail02 = "@datason.com.mx";
        public const string ServerMail02 = "datason.com.mx";
        public const int PortMail02 = 587;
        public const string UserNameMail02 = "@datason.com.mx";
        public const string PasswordMail02 = "";

        // Registro Inicial Publico en GENERAL Organizacion

        public const string PgRfc = "PGE010101AAA";
        public const string PgRazonSocial = "Publico en General";
        public const int PgEstado = 3;  // En caso de no quere que se utilice poner 2
        public const bool PgStatus = true;
        public const string PgMail = "peg@peg.com";

        // Registro Usuario Publico en GENERAL

        public const string DeNombreMailPublico = "Publico";
        public const string DeMailPublico = "publico@datason.com.mx";
        public const int EstadoPublico = 3;
        public const int NivelPublico = 1;
        public const string UserNameMailPublico = "publico@datason.com.mx";
        public const string PasswordMailPublico = "PublicoLibre1.";


        // Registro de Sistema
        public const string SyRfc = "WEB010101MAS";
        public const string SyRazonSocial = "WEBMASTER";
        public const int SyEstado = 2;
        public const bool SyStatus = true;
        public const string SyMail = "info@datason.com.mx";
        public const string SysPassword = "24680212Ih.";

        public const string Arranque = "2.468022";

        public const string OrgTipo = "Administracion,Cliente,Proveedor";
        public const string Niveles = "Registrado,Proveedor,Cliente,Cliente_Admin,Zuver,Zuver_Admin";

        public const bool EsNecesarioConfirmarMail = false;
        public const string ConfirmarMailTxt = "https://datason.com.mx/Account/ConfirmEmail/Id=";
        public const string SitioDesc = "Este sitio genera informacion sobre los reportes de Seguro Social e Infonavit";
    }
}

