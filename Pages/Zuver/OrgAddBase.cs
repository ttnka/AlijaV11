using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Zuver
{
    public class OrgAddBase : ComponentBase
    {
        public const string TBita = "Agregar Organizaciones";

        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgRepo { get; set; } = default!;
        [Inject]
        public Repo<Z110_User, ApplicationDbContext> UserRepo { get; set; } = default!;
        [Inject]
        public Repo<Z180_EmpActiva, ApplicationDbContext> ConfRepo { get; set; } = default!;
        [Inject]
        public IEnviarMail RenviarMail { get; set; } = default!;
        [Inject]
        public IAddUser AddUserRepo { get; set; } = default!;
        [Inject]
        public Repo<ZConfig, ApplicationDbContext> configRepo { get; set; } = default!; 

        [Parameter]
        public Dictionary<string, string> DicData { get; set; } =
            new Dictionary<string, string>();
        [Parameter]
        public List<KeyValuePair<string, string>> TipoOrgs { get; set; } =
            new List<KeyValuePair<string, string>>();

        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        public List<Z100_Org> LasOrgsCorp { get; set; } = new List<Z100_Org>();

        [Parameter]
        public List<Z110_User> LosUsres { get; set; } = new List<Z110_User>();

        
        [Parameter]
        public EventCallback LeerOrgAndUserAll { get; set; }
        
        [Parameter]
        public EventCallback OcultarBoton { get; set; } = default!;
        [Parameter]
        public EventCallback LeerAdmin { get; set; }
        
        [Parameter]
        public bool EsAdministrador { get; set; } = false;

        public LaOrgNew OrgNew { get; set; } = new();
        public bool BotonNuevo = false;
        public string EMsn { get; set; } = "";
        
        public bool ShowGpo { get; set; } = false;
        public bool Editando = false;
        
        protected bool Primera { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (!LasOrgs.Any() || !LosUsres.Any())
                    await LeerOrgAndUserAll.InvokeAsync();
            }
            await Leer();
        }

        public async Task Leer()
        {
            try
            {
                PoblarLasOrgsCorp();

                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                        $"Entro a generar una nueva organizacion, {TBita}",
                        Corporativo, ElUser.OrgId);
                await BitacoraAll(bitaTemp);
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error, No fue posible LEER {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(logTemp);
            }
            
        }

        
        public void PoblarLasOrgsCorp()
        { 
            LasOrgsCorp = LasOrgs.Any() ?
                LasOrgs.Where(x => x.Tipo == "Administracion" && x.Estado == 1 && x.Status == true).ToList()
                : new List<Z100_Org>();     
            
            //LasOrgsCorp = LasOrgs.Where(x => x.Estado == 1).GroupBy(x => x.Corporativo).Select(x => x.First()).ToList();
        }

        public void ChecarFormato()
        {
            EMsn = "";
            // Validar si ya existe el RFC
            EMsn += LasOrgs.Exists(x => x.Rfc.ToUpper() == OrgNew.Rfc.ToUpper()) ?
                "El RFC ya esta registrado! -" : "";
            EMsn += LasOrgs.Exists(x => x.Comercial.ToUpper() == OrgNew.Comercial.ToUpper()) ?
                "El Nombre comercial ya existe! -" : "";
            EMsn += LosUsres.Exists(x => x.OldEmail!.ToUpper() == OrgNew.UserOldEmail.ToUpper()) ?
                "El EMAIL ya existe! -" : "";
        }

        public async Task<ApiRespuesta<LaOrgNew>> Servicio(ServiciosTipos tipo, LaOrgNew org)
        {
            ApiRespuesta<LaOrgNew> resp = new() { Exito = false };
            
            try
            {
                if(org != null )
                {
                    if (tipo == ServiciosTipos.Insert)
                    {
                        if (LasOrgs.Exists(x => x.Rfc.ToUpper() == org.Rfc.ToUpper()))
                        {
                            resp.MsnError.Add("El RFC YA EXISTE!");
                            return resp;
                        }
                        if (EMsn != "")
                        {
                            resp.MsnError.Add(EMsn);
                            return resp;
                        }
                        Z100_Org orgAdd = new();
                        orgAdd.OrgId = org.OrgId;
                        orgAdd.Rfc = org.Rfc;
                        orgAdd.Comercial = org.Comercial;
                        orgAdd.RazonSocial = org.RazonSocial;
                        orgAdd.Moral = org.Moral;
                        orgAdd.NumCliente = org.NumCliente;
                        orgAdd.Tipo = org.Tipo;
                        orgAdd.Corporativo = org.Corporativo;
                        orgAdd.Estado = org.Estado;

                        Z100_Org orgInsert = await OrgRepo.Insert(orgAdd);
                        if (orgInsert == null)
                        {
                            resp.MsnError.Add("No puedo agregarse la empresa");
                            return resp;
                        }
                        else
                        {
                            AddUser eAddUsuario = new();
                            eAddUsuario.Mail = org.UserOldEmail;
                            eAddUsuario.Pass = $"{org.Rfc}_Mm1";
                            eAddUsuario.OrgId = org.OrgId;
                            eAddUsuario.Nombre = org.UserNombre;
                            eAddUsuario.Paterno = org.UserPaterno;
                            eAddUsuario.Materno = org.UserMaterno ?? "";
                            eAddUsuario.Corporativo = org.Corporativo;
                            if (EsAdministrador)
                            {
                                if (org.Tipo == "Cliente")
                                {
                                    eAddUsuario.Nivel = 4;
                                }
                                else if (org.Tipo == "Proveedor")
                                {
                                    eAddUsuario.Nivel = 2;
                                }
                                else
                                {
                                    eAddUsuario.Nivel = 6;
                                }
                            }
                            else
                            {
                                eAddUsuario.Nivel = org.UserNivel;
                            }

                            ApiRespuesta<AddUser> userInsert = await AddUserRepo.CrearNewAcceso(eAddUsuario);
                            if (!userInsert.Exito)
                            {
                                resp.MsnError.Add("No se agrego un usuario de la nueva organizacion");
                                return resp;    
                            }

                            Z110_User nUser = new()
                            {
                                UserId = userInsert.Data.UserId,
                                Nombre = userInsert.Data.Nombre,
                                Paterno = userInsert.Data.Paterno,
                                Materno = string.IsNullOrEmpty(userInsert.Data.Materno) ? "" : userInsert.Data.Materno,
                                Nivel = userInsert.Data.Nivel,
                                OrgId = userInsert.Data.OrgId,
                                OldEmail = userInsert.Data.Mail
                            };
                            ApiRespuesta<Z110_User> uResp = await InsertUser(nUser);
                            if (!uResp.Exito)
                            {
                                resp.Exito = false;
                                foreach (var e in uResp.MsnError)
                                {
                                    resp.MsnError.Add($"{e}");
                                }

                            } else
                            {
                                IEnumerable<ZConfig> emailCampos = await configRepo.Get(x => x.Grupo == "EMail" &&
                                                x.Tipo == "Organizacion" && x.Status == true ) ;
                                emailCampos = emailCampos.OrderByDescending(x => x.Fecha1);
                                MailCampos mc = new();
                                mc.ParaNombre.Add(uResp.Data.Completo);
                                mc.ParaEmail.Add(uResp.Data.OldEmail);
                                mc.Titulo = emailCampos.Any(x=>x.Titulo == "Titulo") ?
                                    emailCampos.FirstOrDefault(x => x.Titulo == "Titulo")!.Txt! :
                                    "Nueva Organizacion en alijadores.com";
                                mc.Titulo += $" {org.Comercial}";
                                mc.Cuerpo = emailCampos.Any(x => x.Titulo == "Cuerpo") ?
                                    emailCampos.FirstOrDefault(x     => x.Titulo == "Cuerpo")!.Txt! :
                                    "Se creo una nueva organizacion en la aplicacion alijadores.com";
                                mc.Cuerpo += $"<br /> Usuario: {mc.ParaEmail} <br /> contraseña: {eAddUsuario.Pass}";

                                var eMail = await RenviarMail.EnviarMail(mc);
                                if (!eMail.Exito) 
                                {
                                    resp.MsnError.Add("No fue posible enviar email de regsitro");
                                }
                            }

                            org.OrgId = orgInsert.OrgId;
                            org.UserId = userInsert.Data.UserId;
                            resp.Data = org;
                            resp.Exito = !resp.MsnError.Any();
                            return resp;
                        }
                    }
                }
                resp.Exito = true;
                return resp;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                            $"Error al intentar agregar una organizacion y un usuario, {TBita}, {ex}",
                            Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                resp.Exito = false;
                return resp;
            }
        }

        public async Task<ApiRespuesta<Z110_User>> InsertUser(Z110_User nUser)
        {
            ApiRespuesta<Z110_User> resultado = new();
            try
            {
                if (nUser != null)
                {
                    Z110_User userInsert = await UserRepo.Insert(nUser);
                    resultado.Exito = true;
                    resultado.Data = userInsert;
                }
                else
                {
                    resultado.Exito = false;
                    resultado.MsnError.Add($"No se pudo insertar el usuario");
                }
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.MsnError.Add(ex.Message);
            }
            return resultado;
        }

        public async Task<ApiRespuesta<Z180_EmpActiva>> EmpActAddUser(Z180_EmpActiva datos)
        {
            ApiRespuesta<Z180_EmpActiva> resultado = new() { Exito = false};
            try
            {
                if (datos == null)
                {
                    resultado.MsnError.Add($"No se agrego Empresa activa ");
                }
                else
                {
                    var resp =  await ConfRepo.Insert(datos);
                    if (resp != null )
                    {
                        resultado.Exito = true;
                        resultado.Data = resp;
                    }
                }
                
                return resultado;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                            $"Error al intentar agregar una organizacion y un usuario, {TBita}, {ex}",
                            Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                return resultado;
            }
        }

        public class LaOrgNew : Z100_Org

        {
            [StringLength(50)]
            public string UserId { get; set; } = "";
            [StringLength(50)]
            public string UserNombre { get; set; } = "";
            [StringLength(50)]
            public string UserPaterno { get; set; } = "";
            [StringLength(50)]
            public string? UserMaterno { get; set; }
            public int UserNivel { get; set; } = 1;

            public string UserOldEmail { get; set; } = "";

            public int UserEstado { get; set; } = 2;
            public bool USerStatus { get; set; } = true;
        }

        #region Usuario y Bitacora

        [CascadingParameter(Name = "CorporativoAll")]
        public string Corporativo { get; set; } = "All";

        [CascadingParameter(Name = "ElUserAll")]
        public Z110_User ElUser { get; set; } = new();

        [Inject]
        public Repo<Z190_Bitacora, ApplicationDbContext> BitaRepo { get; set; } = default!;
        [Inject]
        public Repo<Z192_Logs, ApplicationDbContext> LogRepo { get; set; } = default!;

        public MyFunc MyFunc { get; set; } = new MyFunc();
        public NotificationMessage ElMsn(string tipo, string titulo, string mensaje, int duracion)
        {
            NotificationMessage respuesta = new();
            switch (tipo)
            {
                case "Info":
                    respuesta.Severity = NotificationSeverity.Info;
                    break;
                case "Error":
                    respuesta.Severity = NotificationSeverity.Error;
                    break;
                case "Warning":
                    respuesta.Severity = NotificationSeverity.Warning;
                    break;
                default:
                    respuesta.Severity = NotificationSeverity.Success;
                    break;
            }
            respuesta.Summary = titulo;
            respuesta.Detail = mensaje;
            respuesta.Duration = 4000 + duracion;
            return respuesta;
        }
        [Inject]
        public NavigationManager NM { get; set; } = default!;
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
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar escribir BITACORA, {TBita},{ex}",
                    Corporativo, ElUser.OrgId);
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
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar escribir BITACORA, {TBita},{ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }

        }
        #endregion

        #region Pertenece solo a esta Pagina
        public void Cancelar()
        {
            //OrgNew = new();
            OrgNew.OrgId = "";
            OrgNew.Rfc = "";
            OrgNew.Comercial = "";
            OrgNew.RazonSocial = "";
            OrgNew.NumCliente = "";
            OrgNew.Tipo = "";
            OrgNew.Corporativo = "All";
            OrgNew.UserId = "";
            OrgNew.UserNombre = "";
            OrgNew.UserPaterno = "";
            OrgNew.UserMaterno = "";
            OrgNew.UserOldEmail = "";
            OrgNew.UserNivel = 1;
            ChecarFormato();
            StateHasChanged();
        }

        
        #endregion


    }
}

