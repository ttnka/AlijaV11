using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Alija
{
	public class FacturaListBase : ComponentBase
	{
        public const string TBita = "Facturas";

        [Inject]
        public Repo<Z220_Factura, ApplicationDbContext> FacturaRepo { get; set; } = default!;
        [Inject]
        public Repo<Z222_FactDet, ApplicationDbContext> FactDetRepo { get; set; } = default!;
        [Inject]
        public Repo<Z170_File, ApplicationDbContext> FileRepo { get; set; } = default!;
        // Cascadate
        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        //[Parameter]
        
        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        [Parameter]
        public List<Z170_File> LosArchivosAll { get; set; } = new List<Z170_File>();
        [Parameter]
        public List<Z220_Factura> LasFacturas { get; set; } = new List<Z220_Factura>();
        [Parameter]
        public List<Z200_Folio> LosFolios { get; set; } = new List<Z200_Folio>();

        // Callback
        
        [Parameter]
        public EventCallback ReadLasOrgsAll { get; set; }
        [Parameter]
        public EventCallback<FiltroFactura> ReadLasFacturasAll { get; set; }
        [Parameter]
        public EventCallback<FiltroFolio> ReadLosFoliosAll { get; set; }
        [Parameter]
        public EventCallback ReadLosArchivosAll { get; set; }

        // Variables Dic - Listados - Clases
        protected List<KeyValuePair<int, string>> LosEdos { get; set; } =
            new List<KeyValuePair<int, string>>();
        public List<Z100_Org> LosClientes { get; set; } = new List<Z100_Org>();
        

        protected Z100_Org ElCliente { get; set; } = new();
        

        public RadzenDataGrid<Z220_Factura>? FacturaGrid { get; set; } = new RadzenDataGrid<Z220_Factura>();

        protected string FactEtiqueta { get; set; } = MyFunc.GeneraEtiquetaEstados("Factura");
        protected string[] FactEstadoArray { get; set; } = Constantes.FacturaEstado.Split(",");
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
                FiltroFactura nfa = new();
                if (LasFacturas != null && !LasFacturas.Any())
                    await LeerFacturas(nfa);

                FiltroFolio nfo = new();
                if (LosFolios != null && !LosFolios.Any())
                    await LeerFolios(nfo);

                await LeerClientes();
                await LeerEstadosFacturas();
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



        protected async Task LeerFacturas(FiltroFactura? ff)
        {
            try
            {
                FiltroFactura nf = new()
                {
                    Datos = true,
                    EmpresaId = EmpresaActiva.OrgId
                };
                Leyendo = true;
                if(ff == null || !ff.Datos)
                {   
                    if(ElCliente != null && ElCliente.OrgId.Length > 30)
                    {
                        nf.Datos = true;
                        nf.OrgId = ElCliente.OrgId;
                        nf.EmpresaId = EmpresaActiva.OrgId;
                        await ReadLasFacturasAll.InvokeAsync(nf);
                        Leyendo = false;
                        StateHasChanged();
                        return;
                    }
                    await ReadLasFacturasAll.InvokeAsync(nf);
                    Leyendo = false;
                    StateHasChanged();
                    return;
                }
                await ReadLasFacturasAll.InvokeAsync(ff);
                Leyendo = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos historicos de facturas, {TBita}, {ex}",
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
                        await ReadLosFoliosAll.InvokeAsync(nf);
                        Leyendo = false;
                        StateHasChanged();
                        return;
                    }
                    await ReadLosFoliosAll.InvokeAsync(nf);
                    Leyendo = false;
                    StateHasChanged();
                    return;
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

        protected async Task<string> FacturaTxt(Z220_Factura fact)
        {
            //FolioEstado = "Nuevo,Ejecutado,Facturado,Pagado,Cancelado";
            try
            {
                /*
                string resp = $"Folio: {folio.FolioId}, Fecha: {folio.Fecha} Empresa: ";
                resp += LasOrgs.Any(x => x.OrgId == folio.OrgId) ?
                    LasOrgs.FirstOrDefault(x => x.OrgId == folio.OrgId)!.ComercialRfc : "Sin empresa";
                resp += $"Titulo: {folio.Titulo}, Comentarios: {folio.Obs}";
                var FolioEdo = Constantes.FolioEstado.Split(",");
                resp += $"Estado: {FolioEdo[folio.Estado - 1]} Estatus: ";
                resp += folio.Status ? "Activo" : "Suspendido";
                */
                return "Error";
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar generer TEXTO en funcion FolioTxt de la pagina {TBita}, {ex}",
                        Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                return "Error";
            }
        }

        protected async Task LeerEstadosFacturas()
        {
            try
            {
                for (var i = 0; i < FactEstadoArray.Length; i++)
                {
                    LosEdos.Add(new KeyValuePair<int, string>
                        (i + 1, FactEstadoArray[i]));
                    if (ElUser.Nivel < 6 && i == 1) break;
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer Los estados de las FACTURAS, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task<bool> SigEdoFactura(Z220_Factura fact)
        {
            try
            {
                int valorMinimo = 1; int valor = 0;
                if (fact != null)
                {
                    if (fact.Estado == 1)
                    {
                        // Valor minimo es tener un FOLIOS el cual ya no se podra AGREGAR o MODIFICAR
                        IEnumerable<Z222_FactDet> folios = await FactDetRepo.Get(x =>x.FacturaId == fact.FacturaId &&
                                                            x.Status == true);
                        valor += folios != null && folios.Any() ? 1 : 0;
                        // ARCHIVOS PDF y XLS
                        if (valor == 1)
                        {
                            /*
                            IEnumerable<Z203_Transporte> trans = await TransporteRepo.Get(x => x.FolioId == folio.FolioId &&
                                                            x.Status == true);
                            valor += trans != null && trans.Any() ? 1 : 0
                            */
                        }
                        // REPORTE de HECHOS
                        if (valor == 2)
                        {
                            /*
                            IEnumerable<Z205_Carro> carro = await CarroRepo.Get(x => x.FolioId == folio.FolioId &&
                                                            x.Status == true);
                            valor += carro != null && carro.Any() ? 1 : 0;
                            */
                        }
                        // FOTOS EVIDENCIAS
                        if (valor == 3)
                        {
                            /*
                            IEnumerable<Z204_Empleado> empleado = await EmpleadoRepo.Get(x => x.FolioId == folio.FolioId &&
                                                                x.Status == true);
                            valor += empleado != null && empleado.Any() ? 1 : 0;
                            */
                        }

                    }
                }
                return valor >= valorMinimo;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar EVALUAR SIGUIENTE ESTADO DE FACTURA {fact.FacturaNum}, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                return false;
            }
        }


        protected async Task<ApiRespuesta<Z220_Factura>> Servicio(ServiciosTipos tipo, Z220_Factura fact)
        {
            ApiRespuesta<Z220_Factura> resp = new()
            {
                Exito = false
                
            };

            try
            {
                if (fact != null && EmpresaActiva.OrgId.Length > 30)
                {
                    fact.EmpresaId = EmpresaActiva.OrgId;
                    if (tipo == ServiciosTipos.Insert)
                    {
                        fact.FacturaId = Guid.NewGuid().ToString();
                        fact.Estado = 1;
                        Z220_Factura factInsert = await FacturaRepo.Insert(fact);
                        if (factInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = factInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro FACTURA {fact.FacturaNum}");
                        }
                        return resp;
                    }
                    else if (tipo == ServiciosTipos.Update)
                    {
                        Z220_Factura factUpdate = await FacturaRepo.Update(fact);
                        if (factUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = factUpdate;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Actualizo el registro FACTURA {fact.Fecha} Fact Num:{fact.FacturaNum}");
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

