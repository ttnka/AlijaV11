using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using NPOI.SS.Formula.Functions;

namespace DashBoard.Pages.Sistema
{
	public class ArranqueBase : ComponentBase
	{
        public const string TBita = "Arranque";
        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgsRepo { get; set; } = default!;
        [Inject]
        public Repo<ZConfig, ApplicationDbContext> ConfRepo { get; set; } = default!;
        [Inject]
        public IAddUser AddUserRepo { get; set; } = default!;
        [Inject]
        public Repo<Z110_User, ApplicationDbContext> UserRepo { get; set; } = default!;
        

        [Inject]
        public NavigationManager NM { get; set; } = default!;

        public bool Editando = false;
        protected override async Task OnInitializedAsync()
        {
            List<Z100_Org> emp1 = (await OrgsRepo.Get(x => x.Rfc == Constantes.PgRfc)).ToList();
            List<Z100_Org> emp2 = (await OrgsRepo.Get(x => x.Rfc == Constantes.SyRfc)).ToList();
            if (emp1 != null && emp1.Count > 0 && emp2 != null && emp2.Count > 0)
            { NM.NavigateTo("/", true); }
        }

        protected async Task RunInicio()
        {
            Editando = true;
            if (Clave.Pass == Constantes.Arranque)
            {
                Editando = false;
                await Creacion();
                await AgregarCamposRequeridos();
                await AgregaTextoMails();
            }
            Clave.Pass = "";
            NM.NavigateTo("/");
        }
        protected void PassW()
        {
            if (Clave.Pass == Constantes.Arranque)
                Editando = true;
        }
        protected async Task<bool> Creacion()
        {
            IEnumerable<Z100_Org> resultado = await OrgsRepo.GetAll();
            if (resultado == null || resultado.Any()) return false;
            try
            {
                List<string> Errores = new();
                string corporativo = Guid.NewGuid().ToString();
                // Genera una nueva organizacion con datos sistema 
                Z100_Org SysOrg = new()
                {
                    OrgId = corporativo,
                    Rfc = Constantes.SyRfc,
                    Comercial = Constantes.SyRazonSocial,
                    RazonSocial = Constantes.SyRazonSocial,
                    Corporativo = corporativo,
                    Estado = Constantes.SyEstado,
                    Status = Constantes.SyStatus,

                    Tipo = "Administracion"
                };
                Z100_Org newSysOrg = await OrgsRepo.Insert(SysOrg);

                
                // Genera un nuevo acceso al sistema con un usuario
                AddUser eAddUsuario = new()
                {
                    Mail = Constantes.SyMail,
                    Pass = Constantes.SysPassword,
                    OrgId = newSysOrg.OrgId,
                    Nombre = "El WebMaster",
                    Paterno = Constantes.ElDominio,
                    Materno = "Inc",
                    Corporativo = corporativo,
                    Sistema = true,
                    Nivel = Constantes.Nivel01
                };

                ApiRespuesta<AddUser> userNew = await AddUserRepo.CrearNewAcceso(eAddUsuario);

                if (userNew.Exito)
                {
                    var axUTmp = userNew.Data;
                    Z110_User userTmp = new() {
                        UserId = axUTmp.UserId,
                        OldEmail = axUTmp.Mail,
                        Nombre = axUTmp.Nombre,
                        Paterno = axUTmp.Paterno,
                        Materno = axUTmp.Materno ?? "",
                        Nivel = axUTmp.Nivel,
                        OrgId = axUTmp.OrgId
                    };
                    var res = await UserRepo.Insert(userTmp) ??
                        throw new Exception("No se creo el Usuario Inicial");
;
                }
                
                // Genera una organizacion nueva para publico en general
                Z100_Org PgOrg = new()
                {
                    OrgId = Guid.NewGuid().ToString(),
                    Rfc = Constantes.PgRfc,
                    Comercial = Constantes.PgRazonSocial,
                    RazonSocial = Constantes.PgRazonSocial,
                    Estado = Constantes.PgEstado,
                    Status = Constantes.PgStatus,
                    Corporativo = corporativo,
                    Tipo = "Administracion"
                };
                Z100_Org newPgOrg = await OrgsRepo.Insert(PgOrg);

                // Genera acceso para publico en general 
                AddUser eAddUsuarioPublico = new()
                {
                    Mail = Constantes.DeMailPublico,
                    Pass = Constantes.PasswordMailPublico,
                    OrgId = newPgOrg.OrgId,
                    Nombre = "Publico",
                    Paterno = "General",
                    Materno = "S/F",
                    Corporativo = corporativo,
                    Sistema = true,
                    Nivel = Constantes.NivelPublico
                };

                ApiRespuesta<AddUser> userNewPublico = await AddUserRepo.CrearNewAcceso(eAddUsuarioPublico);
                if (userNewPublico.Exito)
                {
                    var axUTmp = userNewPublico.Data;
                    Z110_User userTmp = new()
                    {
                        UserId = axUTmp.UserId,
                        OldEmail = axUTmp.Mail,
                        Nombre = axUTmp.Nombre,
                        Paterno = axUTmp.Paterno,
                        Materno = axUTmp.Materno ?? "",
                        Nivel = axUTmp.Nivel,
                        OrgId = axUTmp.OrgId
                    };
                    var res = await UserRepo.Insert(userTmp) ??
                        throw new Exception("No se creo el Usuario Publico");
                }
                string txt = $"{TBita}, Se las tablas por primera vez, con 2 empresas nuevas una administrador {Constantes.ElDominio}";
                txt += $" su administrador y otra como empresa donde registrar al publico en general";
                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(eAddUsuario.UserId, eAddUsuario.OrgId,
                    txt, "All", eAddUsuario.OrgId);
                await BitacoraAll(bitaTemp);
                return true;
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog("Sistema"!, "Sistema",
                    $"{TBita}, Error al intentar Arranque de bases de datos {ex}", "All", "Sistema");
                await LogAll(logTemp);
                return false;
            }
        }

        protected async Task AgregarCamposRequeridos()
        {
            try
            {
                List<ZConfig> lista = new List<ZConfig>();
                ZConfig campos = new()
                {
                    ConfigId = Guid.NewGuid().ToString(),
                    Grupo = "CAMPOS", Tipo = "TITULO", Titulo = "Campos", Estado = 1 
                };

                lista.Add(campos);
                List<string> camposTmp = Constantes.CamposAcapurar.Split(",").ToList();
                foreach (var t in camposTmp)
                {
                    ZConfig campoNew = new()
                    {
                        ConfigId = Guid.NewGuid().ToString(),
                        Grupo = "CAMPOS",
                        Tipo = "ELEMENTOS",
                        Titulo = t,
                        Fecha1 = DateTime.Now,
                        Estado = 3
                    };
                    lista.Add(campoNew);
                }

                ZConfig tractorTipo = new()
                {
                    ConfigId = Guid.NewGuid().ToString(), Grupo= "TRACTORTIPO",
                    Tipo= "TITULO", Titulo="Tractor Tipo", Estado = 1
                };
                lista.Add(tractorTipo);

                List<string> tractorTmp = Constantes.TractorTipo.Split(",").ToList(); 
                foreach (var t in tractorTmp)
                {
                    ZConfig tractoNew = new()
                    {
                        ConfigId = Guid.NewGuid().ToString(),
                        Grupo = "TRACTORTIPO",
                        Tipo = "ELEMENTOS",
                        Titulo = t,
                        Fecha1 = DateTime.Now,
                        Estado = 3
                    };
                    lista.Add(tractoNew);
                }

                ZConfig manoTipo = new()
                {
                    ConfigId = Guid.NewGuid().ToString(),
                    Grupo = "MANIOBRATIPO",
                    Tipo = "TITULO",
                    Titulo = "Maniobra Tipo",
                    Estado = 1
                };
                lista.Add(manoTipo);
                List<string> manoTmp = Constantes.ManiobraTipo.Split(",").ToList();
                foreach (var m in manoTmp)
                {
                    ZConfig manoNew = new()
                    {
                        ConfigId = Guid.NewGuid().ToString(),
                        Grupo = "MANIOBRATIPO",
                        Tipo = "ELEMENTOS",
                        Titulo = m,
                        Fecha1 = DateTime.Now,
                        Estado = 3
                    };
                    lista.Add(manoNew);
                }

                if (lista.Any())
                {
                   var varios =  await ConfRepo.InsertPlus(lista);
                    string txt = "Se agrego varios elementos, ";
                    if (varios.Any())
                    {
                        foreach(var t in varios)
                        {
                            txt += $"Grupo: {t.Grupo}, Tipo: {t.Tipo}, Titulo: {t.Titulo}, ";
                        }
                        Z190_Bitacora bita = MyFunc.MakeBitacora("Sistema", "Sistema", txt, "Sistema", "Sistema");
                        await BitacoraAll(bita);
                    }
                }
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog("Sistema"!, "Sistema",
                    $"{TBita}, Error al intentar Arranque agregar campos a configurador {ex}", "All", "Sistema");
                await LogAll(logTemp);
            }
        } 

        protected async Task AgregaTextoMails()
        {
            try
            {
                List<ZConfig> nuevosReg = new List<ZConfig>();
                //  para los mials hay tres tipo de texto, usamos txt para titulo o bien cuerpo
                //  Grupo: Email
                //  Tipo:Organizacion, Usuario, Folio
                //  Titulo: Titulo, Cuerpo
                
                ZConfig t1 = new()
                {
                    ConfigId = Guid.NewGuid().ToString(),
                    Grupo = "EMail",
                    Tipo = "Organizacion",
                    Titulo = "Titulo",
                    Txt = "Bienvenidos a Alijadores.com !",
                    Fecha1 = DateTime.Now
                };
                nuevosReg.Add(t1);

                t1 = new() {
                    ConfigId = Guid.NewGuid().ToString(),
                    Grupo = "EMail",
                    Tipo = "Organizacion",
                    Titulo = "Cuerpo",
                    Txt = "<h1>Saludos tenemos una nueva organizacion!</h1>",
                    Fecha1 = DateTime.Now
                };
                #region Texto de bienvenida
                t1.Txt += "<br /> Esta nueva organizacion se registro con tu correo, puedes consultar en";
                t1.Txt += "<br /> <a href='https://alijadores.com'>Alijadores.com</a> todos nuestros servicios";
                t1.Txt += "<br /> Utiliza esta cuenta de correo como usuario y la contraseña es tu RFC.";
                t1.Txt += "<br /> en tu primera visita es necesario cambiar tu contraseña por seguridad.";
                t1.Txt += "<br /> si tienes dudas te invitamos a contactarnos, via telefonica o bien email";
                t1.Txt += "<br /> webmaster@alijadores.com";
                t1.Txt += "<br /><br /> Atentamente ";
                t1.Txt += "<br /> <b>El equipo de trabajo de Alijadores</b>";
                #endregion
                nuevosReg.Add(t1);

                t1 = new()
                {
                    ConfigId = Guid.NewGuid().ToString(),
                    Grupo = "EMail",
                    Tipo = "Usuario",
                    Titulo = "Titulo",
                    Txt = "Bienvenido a Alijadores.com! tienes un nuevo usuario",
                    Fecha1 = DateTime.Now
                };
                nuevosReg.Add(t1);

                t1 = new()
                {
                    ConfigId = Guid.NewGuid().ToString(),
                    Grupo = "EMail",
                    Tipo = "Usuario",
                    Titulo = "Cuerpo",
                    Txt = "<h1>Saludos tenemos un nuevo usuario con tu nombre!</h1>",
                    Fecha1 = DateTime.Now
                };
                #region Texto de nuevo usuario
                t1.Txt += "<br /> Hola somos <a href='https://alijadores.com'>Alijadores.com</a>, ";
                t1.Txt += "<br /> el administrador de tu empresa, creo un nuevo usuario con tu cuenta de correo";
                t1.Txt += "<br /> puedes utiliza esta cuenta de correo como usuario y la contraseña la tiene tu administrador.";
                t1.Txt += "<br /> puedes solicitar cambiar tu contraseña por seguridad. Para lo cual te enviaremos otro correo";
                t1.Txt += "<br /> si tienes dudas te invitamos a contactarnos, via telefonica o bien email";
                t1.Txt += "<br /> webmaster@alijadores.com";
                t1.Txt += "<br /><br /> Atentamente ";
                t1.Txt += "<br /> <b>El equipo de trabajo de Alijadores</b>";
                #endregion
                nuevosReg.Add(t1);

                t1 = new()
                {
                    ConfigId = Guid.NewGuid().ToString(),
                    Grupo = "EMail",
                    Tipo = "Folio",
                    Titulo = "Titulo",
                    Txt = "Somos Alijadores.com! y tenemos un nuevo folio / factura con tu nombre",
                    Fecha1 = DateTime.Now
                };
                nuevosReg.Add(t1);

                t1 = new()
                {
                    ConfigId = Guid.NewGuid().ToString(),
                    Grupo = "EMail",
                    Tipo = "Folio",
                    Titulo = "Cuerpo",
                    Txt = "<h1>Saludos tenemos un nuevo folio / factura de un nuevo trabajo</h1>",
                    Fecha1 = DateTime.Now
                };
                #region Texto de nuevo Folio / Factura
                t1.Txt += "<br /> Hola somos <a href='https://alijadores.com'>Alijadores.com</a>, ";
                t1.Txt += "<br /> tenemos un nuevo folio / factura de tu empresa ";
                t1.Txt += "<br /> puedes consultar este documento en la liga que anexamos";
                t1.Txt += "<br /> si tienes dudas te invitamos a contactarnos, via telefonica o bien email";
                t1.Txt += "<br /> webmaster@alijadores.com";
                t1.Txt += "<br /><br /> Atentamente ";
                t1.Txt += "<br /> <b>El equipo de trabajo de Alijadores</b>";
                #endregion    
                nuevosReg.Add(t1);

                var varios = await ConfRepo.InsertPlus(nuevosReg);
                string txt = "Se agrego los siguientes campos";
                if (varios.Any())
                {
                    foreach (var t in varios)
                    {
                        txt += $"Grupo: {t.Grupo}, tipo: {t.Tipo}, titulo: {t.Titulo}, Texto: {t.Txt}";
                    }
                    Z190_Bitacora bita = MyFunc.MakeBitacora("Sistema", "Sistema", txt, "Sistema", "Sistema");
                    await BitacoraAll(bita);
                }
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog("Sistema" , "Sistema",
                    $"{TBita}, Error al intentar TEXTO para enviar mails {ex}", "All", "Sistema");
                await LogAll(logTemp);
            }
        }

        public class LaClave
        {
            public string Pass { get; set; } = "";
        }
        public LaClave Clave { get; set; } = new();
        public MyFunc MyFunc { get; set; } = new();

        [Inject]
        public Repo<Z190_Bitacora, ApplicationDbContext> BitaRepo { get; set; } = default!;
        [Inject]
        public Repo<Z192_Logs, ApplicationDbContext> LogRepo { get; set; } = default!;
        public Z190_Bitacora LastBita { get; set; } = new();
        public Z192_Logs LastLog { get; set; } = new();
        public async Task BitacoraAll(Z190_Bitacora bita)
        {
            try
            {
                if (bita.BitacoraId != LastBita.BitacoraId)
                {
                    LastBita = bita;
                    await BitaRepo.Insert(bita);
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog("Sistema", "Sistema",
                    $"Error al intentar iniciar, {TBita},{ex}",
                    "Sistema", "Sistema");
                await LogAll(LogT);
            }
        }

        public async Task LogAll(Z192_Logs log)
        {
            try
            {
                if (log.BitacoraId != LastLog.BitacoraId)
                {
                    LastLog = log;
                    await LogRepo.Insert(log);
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog("Sistema", "Sistema",
                    $"Error al intentar iniciar, {TBita},{ex}",
                    "Sistema", "Sistema");
                await LogAll(LogT);
            }

        }
    }
}

