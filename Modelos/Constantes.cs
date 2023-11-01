
using System;
using System.ComponentModel.DataAnnotations;
using static System.Net.WebRequestMethods;

namespace DashBoard.Modelos
{
	public class Constantes
	{
        public const string ElDominio = "Alijadores.com";
        // Mail 01

        public const string DeNombreMail01 = "WebMaster";
        public const string DeMail01 = "webmaster@alijadores.com";
        public const string ServerMail01 = "mail.omnis.com";
        public const int PortMail01 = 587;
        public const int Nivel01 = 1;
        public const string UserNameMail01 = "webmaster@alijadores.com";
        public const string PasswordMail01 = "Wb.2468022";

        // Mail 02
        public const string DeNombreMail02 = "Administrador";
        public const string DeMail02 = "@zuverworks.com";
        public const string ServerMail02 = "zuverworks.com";
        public const int PortMail02 = 587;
        public const string UserNameMail02 = "info@zuverworks.com";
        public const string PasswordMail02 = "2468022Ih.";

        // Registro Inicial Publico en GENERAL Organizacion

        public const string PgRfc = "PGE010101AAA";
        public const string PgRazonSocial = "Publico en General";
        public const int PgEstado = 3;  // En caso de no quere que se utilice poner 2
        public const bool PgStatus = true;
        public const string PgMail = "peg@peg.com";

        // Registro Usuario Publico en GENERAL

        public const string DeNombreMailPublico = "Publico";
        public const string DeMailPublico = "publico@alijadores.com";
        public const int EstadoPublico = 3;
        public const int NivelPublico = 1;
        public const string UserNameMailPublico = "publico@alijadores.com";
        public const string PasswordMailPublico = "PublicoLibre1.";

        // Registro de Sistema
        public const string SyRfc = "ZME130621FFA";
        public const string SyRazonSocial = "Alijadores del Pacifico";
        public const int SyEstado = 1;
        public const bool SyStatus = true;
        public const string SyMail = "webmaster@alijadores.com";
        public const string SysPassword = "24680212Ih.";

        // Configuracion
        public const string Arranque = "2.468022";

        public const string OrgTipo = "Administracion,Cliente,Proveedor";
        public const string Niveles = "Registrado,Proveedor,Cliente,Cliente_Admin,Alijadores,Alijadores_Admin";

        public const bool EsNecesarioConfirmarMail = false;
        public const string ConfirmarMailTxt = "https://alijadores.com/Account/ConfirmEmail/Id=";
        public const string SitioDesc = "Este sitio genera informacion sobre los reportes de Seguro Social e Infonavit";

        // Registro de Estado de Folios
        public const string FolioEstado = "Nuevo,Ejecutado,Facturado,XCancelado";
        public const string FacturaEstado = "Nueva,Publicada,Abonada,Pagada,XCancelada";
        public const string PagoEstado = "Nuevo,Pagado,Conciliado,XCancelado";
        public const string EmpresaActiva = "Empresa Activa";

        public const string CamposAcapurar = "AgenteAduanal,Pedimento,Factura,Mercancia,Tractor,Caja1,Caja2,Placas,TractorTipo,Transportista,Maniobra,Despachador,CartaPorte,SelloRemovido,SelloColocado,Chofer,Identificacion,Cuadrilla,Obs";
        public const string TractorTipo = "Trailer,Rabon";
        public const string ManiobraTipo = "Palet,Granel";

        public const string TiposdePrecios = "Automatico,Manual";
    }
}

