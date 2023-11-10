using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Alija
{
	public class CamposListBase : ComponentBase
	{
        public const string TBita = "Todos los campos";

        [Inject]
        public Repo<Z209_Campos, ApplicationDbContext> CamposRepo { get; set; } = default!;
        [Inject]
        public Repo<ZConfig, ApplicationDbContext> ConfRepo { get; set; } = default!;

        [Parameter]
        public List<ZConfig> LosConfig { get; set; } = new List<ZConfig>();
        public List<ZConfig> Mostrar { get; set; } = new List<ZConfig>();

        [Parameter]
        public EventCallback ReadConfigAll { get; set; }

        [CascadingParameter(Name = "ElFolioAll")]
        public Z200_Folio ElFolio { get; set; } = new();
        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        public List<Z209_Campos> LosCampos { get; set; } = new List<Z209_Campos>();
        public List<KeyValuePair<string, string>> ManiobrasTipos { get; set; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> TractoresTipos { get; set; } = new List<KeyValuePair<string, string>>();

        public RadzenDataGrid<Z209_Campos>? CamposGrid { get; set; } = new RadzenDataGrid<Z209_Campos>();

        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                await LeerLosCampos();

            }

            await Leer();
            await LeerListados();
        }

        protected async Task Leer()
        {
            try
            {
                LosConfig = (await ConfRepo.Get(x => x.Status == true)).ToList();
                Mostrar = LosConfig.Any() ? LosConfig.Where(x => x.Usuario == ElFolio.EmpresaId && x.Status == true &&
                    x.Grupo == "CAMPOS" && x.Tipo == "MOSTRADOS" && x.SiNo == false).ToList() : new List<ZConfig>();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }
        /*
        protected async Task LeerMostrar1()
        {
            try
            {
                LosConfig = (await ConfRepo.Get(x => x.Status == true)).ToList();
                
                Mostrar = LosConfig.Any() ? LosConfig.Where(x => x.Usuario == ElFolio.OrgId && x.Status == true &&
                    x.Grupo == "CAMPOS" && x.Tipo == "REQUERIDOS").ToList() : new List<ZConfig>();
                
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos del los campos que son requeridos {ElFolio.FolioNum}, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }
        */
        protected async Task LeerListados()
        {
            try
            {
                // Leer los tractorTipo
                IEnumerable<ZConfig> tt = LosConfig.Where(x => x.Grupo == "TRACTORTIPO" &&
                                    x.Tipo == "ELEMENTOS" && x.Status == true);
                if (tt.Any())
                {
                    TractoresTipos.Clear();
                    foreach (var t in tt)
                    {
                        TractoresTipos.Add(new KeyValuePair<string, string>(t.Titulo, t.Titulo));
                    }
                }
                // Leer las ManiobrasTipo

                IEnumerable<ZConfig> mt = LosConfig.Where(x => x.Grupo == "MANIOBRATIPO" &&
                                    x.Tipo == "ELEMENTOS" && x.Status == true);
                if (mt.Any())
                {
                    ManiobrasTipos.Clear();
                    foreach (var m in mt)
                    {
                        ManiobrasTipos.Add(new KeyValuePair<string, string>(m.Titulo, m.Titulo));
                    }
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer los registros de configuraciones, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerLosCampos()
        {
            try
            {
                IEnumerable<Z209_Campos> resp = new List<Z209_Campos>();
                if (ElFolio != null && ElFolio.FolioId.Length > 30)
                {
                    if (ElUser.Nivel > 5)
                    {
                        resp = await CamposRepo.Get(x => x.FolioId == ElFolio.FolioId);
                    }
                    else
                    {
                        resp = await CamposRepo.Get(x => x.FolioId == ElFolio.FolioId && x.Status == true);
                    }
                    
                }
                LosCampos = resp != null && resp.Any() ? resp.ToList() : new List<Z209_Campos>() ;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos del los campos de este folio {ElFolio.FolioNum}, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task<ApiRespuesta<Z209_Campos>> Servicio(ServiciosTipos tipo, Z209_Campos campo)
        {
            ApiRespuesta<Z209_Campos> resp = new()
            {
                Exito = false
            };

            try
            {
                if (campo != null)
                {
                    campo.FolioId = ElFolio.FolioId;
                    if (tipo == ServiciosTipos.Insert)
                    {
                        campo.CampoId = Guid.NewGuid().ToString();
                        campo.Estado = 1;
                        Z209_Campos campoInsert = await CamposRepo.Insert(campo);
                        if (campoInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = campoInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro de los campos del folio {ElFolio.FolioNum}");
                        }
                        return resp;
                    }
                    else if (tipo == ServiciosTipos.Update)
                    {
                        Z209_Campos campoUpdate = await CamposRepo.Update(campo);
                        if (campoUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = campoUpdate;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Actualizo el registro los campos {ElFolio.FolioNum}");
                            resp.Exito = false;
                        }
                        return resp;
                    }
                }
                resp.MsnError.Add($"Ningua operacion se realizo! {TBita}");
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

