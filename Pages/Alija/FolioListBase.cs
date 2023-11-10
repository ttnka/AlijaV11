using System;
using System.Collections.Generic;
using DashBoard.Data;
using DashBoard.Modelos;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Components;
using Org.BouncyCastle.Math;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Alija
{
    public class FolioListBase : ComponentBase
    {
        public const string TBita = "Folios Listado";

        [Inject]
        public Repo<ZConfig, ApplicationDbContext> ConfRepo { get; set; } = default!;
        [Inject]
        public Repo<Z170_File, ApplicationDbContext> FileRepo { get; set; } = default!;
        [Inject]
        public Repo<Z200_Folio, ApplicationDbContext> FolioRepo { get; set; } = default!;
        [Inject]
        public Repo<Z201_FolioPrint, ApplicationDbContext> PrintRepo { get; set; } = default!;
        [Inject]
        public Repo<Z209_Campos, ApplicationDbContext> CamposRepo { get; set; } = default!;
        [Inject]
        public Repo<Z210_Concepto, ApplicationDbContext> ConceptoRepo { get; set; } = default!;


        // Cascading
        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        //[Parameter]
        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        [Parameter]
        public IEnumerable<Z200_Folio> LosFolios { get; set; } = new List<Z200_Folio>();

        // CallBack
        [Parameter]
        public EventCallback<FiltroFolio> ReadLosFoliosAll { get; set; }
        [Parameter]
        public EventCallback ReadLasOrgsAll { get; set; }
        

        // Listas y clases
        protected List<KeyValuePair<int, string>> LosEdos { get; set; } =
            new List<KeyValuePair<int, string>>();
        public List<Z100_Org> LosClientes { get; set; } = new List<Z100_Org>();
        protected Z100_Org ElCliente { get; set; } = new();

        protected List<Z170_File> LosArchivosAll { get; set; } = new List<Z170_File>();

        public RadzenDataGrid<Z200_Folio>? FoliosGrid { get; set; } = new RadzenDataGrid<Z200_Folio>();

        protected string EstadoEtiqueta { get; set; } = MyFunc.GeneraEtiquetaEstados("Folio");
        protected string[] EstadoArray { get; set; } = Constantes.FolioEstado.Split(",");
        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;
        

        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (!LasOrgs.Any())
                    await ReadLasOrgsAll.InvokeAsync();
            }
            
            await Leer();
        }

        protected async Task Leer()
        {
            try
            {
                await LeerClientes();
                FiltroFolio nf = new();
                if (!LosFolios.Any())
                    await LeerFolios(nf);

                await LeerEstadosFolios();
                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                     $"Consulto la seccion de {TBita}", Corporativo, ElUser.OrgId);
                await BitacoraAll(bitaTemp);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task ReadFileListAll(string folioId)
        {
            try
            {
                IEnumerable<Z170_File> resp = await FileRepo.Get(x => x.Status == (ElUser.Nivel > 6 ? x.Status : true) &&
                                    x.FolioId == folioId);
                LosArchivosAll = resp.Any() ? resp.ToList() : new List<Z170_File>();    
                
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer archivos registrados en la base de datos y servidor, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }
        
        protected async Task LeerFolios(FiltroFolio? ff)
        {
            try
            {
                FiltroFolio nf = new()
                {
                    Datos = true,
                    EmpresaId = EmpresaActiva.OrgId
                };
                Leyendo = true;
                if (ff == null || !ff.Datos)
                {
                    if (ElCliente != null && ElCliente.OrgId.Length > 30)
                    {
                        nf.Datos = true;
                        nf.OrgId = ElCliente.OrgId;
                        nf.EmpresaId = EmpresaActiva.OrgId;
                        await ReadLosFoliosAll.InvokeAsync(nf);
                        Leyendo = false;
                        StateHasChanged();
                        return;
                    }
                    await ReadLosFoliosAll.InvokeAsync();
                    Leyendo = false;
                    StateHasChanged();
                }

                await ReadLosFoliosAll.InvokeAsync(ff);
                Leyendo = false;
                StateHasChanged();
                
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer LOS FOLIOS, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task<string> LeerPrintFolios(string fId)
        {
            string resp = "";
            try
            {
                if (!string.IsNullOrEmpty(fId))
                {
                    IEnumerable<Z201_FolioPrint> rTmp = await PrintRepo.Get(x => x.FolioId == fId);
                    resp = rTmp != null && rTmp.Any() ? rTmp.FirstOrDefault()!.LlaveId : "";
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
            return resp;
        }

        protected async Task LeerClientes()
        {
            try
            {
                if (LasOrgs.Any())
                    LosClientes = LasOrgs.Where(x => x.Tipo == "Cliente" && x.Status == true).ToList();  
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer Los Clientes, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected Z200_Folio PassToOldFolio(Z200_Folio oldFolio)
        {
            Z200_Folio resp = new();
            resp.FolioId = oldFolio.FolioId.ToString();
            resp.FolioNum = oldFolio.FolioNum;
            resp.OrgId = oldFolio.OrgId.ToString();
            resp.Fecha = oldFolio.Fecha;
            resp.Titulo = oldFolio.Titulo.ToString();
            resp.Obs = oldFolio.Obs ?? "";
            resp.Corporativo = oldFolio.Corporativo.ToString();
            resp.EmpresaId = oldFolio.EmpresaId.ToString();
            resp.Estado = oldFolio.Estado;
            resp.Status = oldFolio.Status;

            return resp; 
        }

        protected async Task LeerEstadosFolios()
        {
            try
            {
                for (var i = 0; i < EstadoArray.Length; i++)
                {
                    LosEdos.Add(new KeyValuePair<int, string>
                        (i + 1, EstadoArray[i]));
                    if (ElUser.Nivel < 7 && i == 1) break;
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer Los ESTADOS DE FOLIOS, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task<ApiRespValor> SigEdoFolio(Z200_Folio folio)
        {
            ApiRespValor resp = new() { Exito = false };
            bool hayErr = false;
            try
            {
                var concepTmp = await ConceptoRepo.Get(x => x.FolioId == folio.FolioId && x.Status == true);
                if (!concepTmp.Any() && concepTmp.Sum(x => x.Importe) < 1)
                {
                    resp.MsnError.Add("No hay conceptos capturados en este folio, captura alguno!");
                    return resp;
                }

                var campoTmp = await CamposRepo.Get(x => x.FolioId == folio.FolioId && x.Status == true);
                if (!campoTmp.Any())
                {
                    resp.MsnError.Add("No hay informacion de la operacion, necesitamos detalles captura todos los campos!");
                    return resp;
                }

                List<string> camposList = Constantes.CamposAcapurar.Split(",").ToList();

                var noRequeridos = await ConfRepo.Get(x => x.SiNo == false && x.Usuario == folio.EmpresaId &&
                        x.Grupo == "CAMPOS" && x.Tipo == "MOSTRADOS" && x.Status == true);
                foreach(var campo1 in camposList)
                {
                    if (noRequeridos.Any(x => x.Titulo == campo1)) continue;
                    var prop = campoTmp.GetType().GetProperty(campo1);
                    if (prop != null )
                    {
                        var valor = prop.GetValue(campo1, null);
                        if (valor != null && string.IsNullOrEmpty(valor.ToString()))
                        {
                            resp.MsnError.Add($"Falta informacion de {campo1} capturala ");
                            hayErr = true;
                        } 
                    }
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar EVALUAR SIGUIENTE ESTADO DE FOLIO {folio.FolioNum}, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                resp.MsnError.Add(ex.Message);
                hayErr = true;
            }
            resp.Exito = !hayErr;
            return resp;
        }

        protected async Task<ApiRespuesta<Z200_Folio>> Servicio(ServiciosTipos tipo, Z200_Folio folio)
        {
            ApiRespuesta<Z200_Folio> resp = new()
            {
                Exito = false
                
            };

            try
            {
                if (folio != null && EmpresaActiva.OrgId.Length > 30)
                {
                    folio.EmpresaId = EmpresaActiva.OrgId;
                    if (tipo == ServiciosTipos.Insert)
                    {
                        folio.FolioId = Guid.NewGuid().ToString();
                        folio.FolioNum = await FolioRepo.GetCount() + 1;
                        folio.Estado = 1;
                        
                        folio.Corporativo = LasOrgs.FirstOrDefault(x => x.OrgId == folio.OrgId)!.Corporativo;
                        
                        Z200_Folio folioInsert = await FolioRepo.Insert(folio);
                        if (folioInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = folioInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro {folio.FolioNum}");
                        }
                        return resp;
                    }
                    else if (tipo == ServiciosTipos.Update)
                    {
                        
                        Z200_Folio folioUpdate = await FolioRepo.Update(folio);
                        
                        if (folioUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = folioUpdate;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Actualizo el registro {folio.Fecha} {folio.FolioId}");
                            resp.Exito = false;
                        }
                        return resp;
                    }
                    else if (tipo == ServiciosTipos.FolioQr)
                    {
                        Z201_FolioPrint nfp = new() {
                            LlaveId = Guid.NewGuid().ToString(),
                            FolioId = folio.FolioId
                        };
                        Z201_FolioPrint respNfp = await PrintRepo.Insert(nfp);
                        if (respNfp != null)
                        {
                            resp.Exito = true;
                            resp.Data = folio;
                        }
                        else
                        {
                            resp.MsnError.Add("No fue posible generar el registro del Qr");
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

