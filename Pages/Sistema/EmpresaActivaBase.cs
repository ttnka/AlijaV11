using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Sistema
{
	public class EmpresaActivaBase : ComponentBase
	{
        public const string TBita = Constantes.EmpresaActiva;
        
        [Inject]
        public Repo<Z180_EmpActiva, ApplicationDbContext> EmpActRepo { get; set; } = default!;


        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        [Parameter]
        public List<Z110_User> LosUsers { get; set; } = new List<Z110_User>();
        [Parameter]
        public List<Z180_EmpActiva> LasEmpHist { get; set; } = new List<Z180_EmpActiva>();

        [Parameter]
        public EventCallback ReadEmpresaActivaAll { get; set; }
        [Parameter]
        public EventCallback ReadLosUsersAll { get; set; }
        [Parameter]
        public EventCallback<Z180_EmpActiva> ReadHistEmpActivaAll { get; set; }

        public List<Z100_Org> LasAlijadoras { get; set; } = new List<Z100_Org>();
        public List<Z110_User> LosEmpleados { get; set; } = new List<Z110_User>();

        public RadzenDataGrid<Z180_EmpActiva>? EmpActGrid { get; set; } = new RadzenDataGrid<Z180_EmpActiva>();
        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (!LasEmpHist.Any())
                    await LeerHistEmps();
            }
            
            await Leer();
        }

        protected async Task Leer()
        {
            try
            {

                await LeerAlijadores();
                await LeerEmpAlijadores();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerAlijadores()
        {
            try
            {
                LasAlijadoras = LasOrgs.Any(x =>x.Tipo == "Administracion" && x.Estado == 1) ? 
                    LasOrgs.Where(x => x.Tipo == "Administracion" &&
                                x.Estado == 1).ToList() :
                    LasAlijadoras ;
                
            }
            catch (Exception ex)
            {
                string txt = $"Error al intentar leer Alijadores de {TBita}, {ex} ";
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId, txt,
                    Corporativo, ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        protected async Task LeerEmpAlijadores()
        {
            try
            {
                List<Z110_User> resp = new List<Z110_User>(); 
                if (LosUsers.Any() && LasAlijadoras.Any())
                {
                    resp = LosUsers.Where(user =>
                                    LasAlijadoras.Select(org => org.OrgId).Contains(user.OrgId)).ToList();

                }
                LosEmpleados = resp.Any() ? resp.ToList() : new List<Z110_User>();
            }
            catch (Exception ex)
            {
                string txt = $"Error al intentar leer historial de {TBita}, {ex} ";
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId, txt,
                    Corporativo, ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        protected async Task LeerHistEmps()
        {
            try
            {
                await ReadEmpresaActivaAll.InvokeAsync();
            }
            catch (Exception ex)
            {
                string txt = $"Error al intentar leer historial de {TBita}, {ex} ";
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId, txt,
                    Corporativo, ElUser.OrgId);
                await LogAll(logTemp);
            }
        }


        protected async Task<ApiRespuesta<Z180_EmpActiva>> Servicio(ServiciosTipos tipo, Z180_EmpActiva empAct)
        {
            ApiRespuesta<Z180_EmpActiva> resp = new() { Exito = false};

            try
            {
                if (empAct != null)
                {
                    if (tipo == ServiciosTipos.Insert)
                    {
                        empAct.EmpActId = Guid.NewGuid().ToString();
                        empAct.Fecha = DateTime.Now;
                        
                        Z180_EmpActiva empActInsert = await EmpActRepo.Insert(empAct);
                        if (empActInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = empActInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro {TBita}");
                            resp.Data = empAct;
                        }
                        return resp;
                    }
                    else if (tipo == ServiciosTipos.Update)
                    {
                        resp.MsnError.Add("No hay servicio de update");
                        // No hay servicio de update
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
                        Corporativo, ElUser.UserId);
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

