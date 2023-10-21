using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Sistema
{
	public class EmpresaActivaBase : ComponentBase
	{
        public const string TBita = Constantes.EmpresaActiva;
        [Inject]
        public Repo<ZConfig, ApplicationDbContext> ConfigRepo { get; set; } = default!;

        [CascadingParameter(Name = "CorporativoAll")]
        public string Corporativo { get; set; } = "All";

        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        [Parameter]
        public List<Z100_Org> LasAlijadoras { get; set; } = new List<Z100_Org>();

        public List<ZConfig> LasConfig { get; set; } = new List<ZConfig>();

        public RadzenDataGrid<ZConfig>? ConfigGrid { get; set; } = new RadzenDataGrid<ZConfig>();
        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (!LasConfig.Any())
                    await LeerConfigs();
            }
            
            await Leer();
        }

        protected async Task Leer()
        {
            try
            {
                await LeerAlijadores();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task LeerAlijadores()
        {
            try
            {
                LasAlijadoras = LasOrgs.Any(x =>x.Tipo == "Administracion" &&
                                x.Estado == 1) ? 
                    LasOrgs.Where(x => x.Tipo == "Administracion" &&
                                x.Estado == 1).ToList() :
                    LasAlijadoras ;
                
            }
            catch (Exception ex)
            {
                string txt = $"Error al intentar leer Alijadores de {TBita}, {ex} ";
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId, txt,
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(logTemp);
            }
        }

        protected async Task LeerConfigs()
        {
            try
            {
                IEnumerable<ZConfig> resp = await ConfigRepo.Get(x => x.Usuario == ElUser.UserId && x.Titulo == TBita);
                if (resp.Any())
                    LasConfig = resp.Any() ? resp.OrderByDescending(x => x.Fecha1).ToList() : LasConfig;
            }
            catch (Exception ex)
            {
                string txt = $"Error al intentar leer historial de {TBita}, {ex} ";
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId, txt,
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(logTemp);
            }
        }

        protected async Task<ApiRespuesta<ZConfig>> Servicio(string tipo, ZConfig zConf)
        {
            ApiRespuesta<ZConfig> resp = new()
            {
                Exito = false,
                Data = zConf
            };

            try
            {
                if (zConf != null)
                {
                    if (tipo == "Insert")
                    {
                        zConf.ConfigId = Guid.NewGuid().ToString();
                        zConf.Fecha1 = DateTime.Now;
                        zConf.Usuario = ElUser.UserId;
                        zConf.Titulo = TBita;

                        ZConfig configInsert = await ConfigRepo.Insert(zConf);
                        if (configInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = configInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro {TBita}");
                            resp.Data = zConf;
                        }
                        return resp;
                    }
                    else if (tipo == "Update")
                    {
                        resp.MsnError.Add("No hay servicio de update");
                        // No hay servicio de update
                    }
                    else if(tipo == "Read")
                    {
                        if(LasConfig.Any(x=>x.Titulo == TBita && x.Usuario == ElUser.UserId))
                        {
                            resp.Data = LasConfig.Where(x => x.Titulo == TBita && x.Usuario == ElUser.UserId)
                                .OrderByDescending(x => x.Fecha1).FirstOrDefault()!;
                            resp.Exito = true;
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
                        Corporativo, ElUser.UserId);
                await LogRepo.Insert(LogT);
                return resp;
            }
        }

        #region Usuario y Bitacora

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
            switch (tipo.ToLower())
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
        public async Task BitacoraAll(Z190_Bitacora bita)
        {
            try
            {
                if (bita.Fecha.Subtract(LastBita.Fecha).TotalSeconds > 15 ||
                    LastBita.Desc != bita.Desc || LastBita.Sistema != bita.Sistema ||
                    LastBita.UserId != bita.UserId || LastBita.OrgId != bita.OrgId)
                {
                    LastBita = bita;
                    await BitaRepo.Insert(bita);
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar escribir BITACORA, {TBita},{ex}",
                    Corporativo, ElUser.UserId);
                await LogRepo.Insert(LogT);
            }

        }
        #endregion

    }
}

