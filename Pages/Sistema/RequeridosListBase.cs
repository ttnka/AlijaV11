using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Sistema
{
	public class RequeridosListBase : ComponentBase
	{
        public const string TBita = "Campos requeridos";

        [Inject]
        public Repo<ZConfig, ApplicationDbContext> ConfRepo { get; set; } = default!;


        // Cascading
        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        //[Parameter]
        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        [Parameter]
        public List<ZConfig> LosConfigs { get; set; } = new List<ZConfig>();


        // CallBack

        [Parameter]
        public EventCallback ReadEmpresaActivaAll { get; set; }
        [Parameter]
        public EventCallback ReadLasOrgsAll { get; set; }
        [Parameter]
        public EventCallback ReadLasConfigAll { get; set; }

        // Listas y clases
        public List<ZConfig> LosCampos { get; set; } = new List<ZConfig>();
        public List<ZConfig> LosDatos { get; set; } = new List<ZConfig>();
        

        
        public List<Z100_Org> LasAdmins { get; set; } = new List<Z100_Org>();

        public RadzenDataGrid<ZConfig>? CamposGrid { get; set; } = new RadzenDataGrid<ZConfig>();

        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (EmpresaActiva == null || EmpresaActiva.OrgId.Length < 15)
                    await ReadEmpresaActivaAll.InvokeAsync();
            }

            await Leer();
        }

        protected async Task Leer()
        {
            try
            {
                if (!LosConfigs.Any()) { await ReadLasConfigAll.InvokeAsync(); }
                await LeerLosCampos();

                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                     $"Consulto la seccion de {TBita}", Corporativo, ElUser.OrgId);
                await BitacoraAll(bitaTemp);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos Campos al inicio, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerLosCampos()
        {
            try
            {
                if (!LasOrgs.Any()) { await ReadLasOrgsAll.InvokeAsync(); }

                LasAdmins = LasOrgs.Any() ? LasOrgs.Where(x => x.Tipo == "Administracion" && x.Estado == 1 && x.Status == true).ToList() :
                    new List<Z100_Org>();
                
                LosCampos = LosConfigs.Any() ? LosConfigs.Where(x => x.Grupo == "CAMPOS" && x.Tipo == "ELEMENTOS").OrderBy(x=>x.Titulo).ToList() :
                    new List<ZConfig>();
                LosDatos = LosConfigs.Any() ? LosConfigs.Where(x => x.Grupo == "CAMPOS" && x.Tipo == "MOSTRADOS").ToList() :
                    new List<ZConfig>();

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos LOS CAMPOS, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerLosConf()
        {
            try
            {
                await ReadLasConfigAll.InvokeAsync();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer los registros de configuraciones, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        

        protected async Task<ApiRespuesta<ZConfig>> ExisteRegistro(ZConfig nuevo)
        {
            ApiRespuesta<ZConfig> respuesta = new();
            try
            {
                IEnumerable<ZConfig> temp = await ConfRepo.Get(x => x.Usuario == nuevo.Usuario && x.Titulo == nuevo.Titulo);
                if (temp.Any())
                {
                    respuesta.Data = temp.OrderByDescending(x => x.Fecha1).FirstOrDefault()!;
                    respuesta.Exito = true;
                }
                else
                {
                    respuesta.Exito = false;
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer los registros de configuraciones ya existentes, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                respuesta.Exito = false;
                respuesta.MsnError.Add(ex.Message);
            }
            return respuesta;
        }

        protected async Task<ApiRespuesta<ZConfig>> Servicio(ServiciosTipos tipo, ZConfig config)
        {
            // USUARIO = EmpresaAdmin Titulo = Campo   Si/No Fecha
            ApiRespuesta<ZConfig> resp = new()
            {
                Exito = false
            };

            try
            {
                if (config != null)
                {
                    if (tipo == ServiciosTipos.Insert)
                    {
                        config.ConfigId = Guid.NewGuid().ToString();

                        ZConfig configInsert = await ConfRepo.Insert(config);
                        if (configInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = configInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro {config.Titulo}");
                        }
                        return resp;
                    }
                    else if (tipo == ServiciosTipos.Update)
                    {

                        
                        ZConfig configUpdate = new();
                        if (configUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = configUpdate;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Actualizo el registro {config.Titulo}");
                            resp.Exito = false;
                        }
                        return resp;
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

