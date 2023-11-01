using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Zuver
{
	public class OrgListBase : ComponentBase
	{
        public const string TBita = "Organizaciones";
        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgRepo { get; set; } = default!;

        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        public List<Z100_Org> LasOrgsCorp { get; set; } = new List<Z100_Org>();
        [Parameter]
        public List<Z110_User> LosUsuarios { get; set; } = new List<Z110_User>();

        
        [Parameter]
        public List<KeyValuePair<int, string>> NivelesEdit { get; set; } =
            new List<KeyValuePair<int, string>>();

        [Parameter]
        public List<KeyValuePair<string, string>> TipoOrgs { get; set; } =
            new List<KeyValuePair<string, string>>();
        
        [Parameter]
        public EventCallback LeerOrgAll { get; set; }
        [Parameter]
        public EventCallback LeerUsersAll { get; set; }

        public RadzenDataGrid<Z100_Org>? OrgGrid { get; set; } =
            new RadzenDataGrid<Z100_Org>();

        
        protected bool Editando = false;
        protected bool ShowAdd { get; set; } = false;
        protected string txtNewOrg = "Nueva Empresa";
        protected bool Primera = true;
        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (!LasOrgs.Any() || !LosUsuarios.Any())
                    await LeerOrgsAndUsers();
            }
            await Leer();
        }

        public async Task Leer()
        {
            try
            {
                await PoblarLasOrgsCorp();

                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                    $"El usuario consulto listado de {TBita}", Corporativo, ElUser.OrgId);
                await BitacoraAll(bitaTemp);
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar generar LEER inicio, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task PoblarLasOrgsCorp()
        {
            try
            {
                
                LasOrgsCorp = LasOrgs;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar generar el listado de corporativos, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerOrgsAndUsers()
        {
            Primera = false;
            await LeerOrgAll.InvokeAsync();
            await LeerUsersAll.InvokeAsync();
        }

        protected async Task GoLeerOrgs()
        {
            await LeerOrgAll.InvokeAsync();
        }

        protected async Task GoLeerUsers()
        {
            await LeerUsersAll.InvokeAsync();
        }

        public async Task<ApiRespuesta<Z100_Org>> Servicio(ServiciosTipos tipo, Z100_Org org)
        {
            ApiRespuesta<Z100_Org> resp = new()
            {
                Exito = false,
                Data = org
            };

            try
            {
                if (org != null )
                {
                    if (tipo == ServiciosTipos.Insert)
                    {
                        org.OrgId = Guid.NewGuid().ToString();
                        Z100_Org orgInsert = await OrgRepo.Insert(org);
                        if (orgInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = orgInsert;
                        }
                        else
                        {
                            resp.MsnError.Add("No se Inserto el registro ");
                        }
                        return resp;
                    }
                    else if (tipo == ServiciosTipos.Insert)
                    {
                        if (LasOrgs.Exists(x=>x.Rfc.ToUpper() == org.Rfc.ToUpper()
                        && x.OrgId != org.OrgId))
                        {
                            resp.MsnError.Add("El RFC ya esta REGISTRADO!");
                            resp.Exito = false;
                            return resp;
                        }
                        Z100_Org orgUpdate = await OrgRepo.Update(org);
                        if (orgUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = orgUpdate;
                            return resp;
                        }
                        else
                        {
                            resp.MsnError.Add("No se Actualizo el registro ");
                            resp.Exito = false;
                            return resp;
                        }
                    }
                }
                resp.MsnError.Add("Ningua operacion se realizo!");
                return resp;
            }
            catch (Exception ex)
            {
                resp.MsnError.Add(ex.Message);
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar {tipo} los registros de {TBita} {ex}",
                        Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                return resp;
            }

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

    }
}

