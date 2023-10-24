using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Alija
{
	public class FactDetListBase : ComponentBase
	{
        public const string TBita = "Factura Detalle";

        [Inject]
        public Repo<Z220_Factura, ApplicationDbContext> FacturaRepo { get; set; } = default!;
        [Inject]
        public Repo<Z222_FactDet, ApplicationDbContext> DetRepo { get; set; } = default!;
                                    

        [CascadingParameter(Name = "LaFacturaAll")]
        public Z220_Factura LaFactura { get; set; } = new();
        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        [Parameter]
        public List<Z200_Folio> LosFoliosAll { get; set; } = new List<Z200_Folio>();


        [Parameter]
        public EventCallback<FiltroFolio> ReadLosFolios { get; set; }

        public List<Z222_FactDet> LosDets { get; set; } = new List<Z222_FactDet>();
        public List<Z200_Folio> LosFolios { get; set; } = new List<Z200_Folio>();



        public RadzenDataGrid<Z222_FactDet>? DetalleGrid { get; set; } = new RadzenDataGrid<Z222_FactDet>();

        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (LosDets != null  && !LosDets.Any())
                    await LeerFactDets();
            }

            await Leer();
        }

        protected async Task Leer()
        {
            try
            {
                await LeerFolios();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task LeerFactDets()
        {
            try
            {
                IEnumerable<Z222_FactDet> resp = new List<Z222_FactDet>();
                if (LaFactura != null && LaFactura.FacturaId.Length > 30)
                {
                    resp = await DetRepo.Get(x => x.FacturaId == LaFactura.FacturaId);
                }
                LosDets = resp != null && resp.Any() ? resp.ToList() : new List<Z222_FactDet>();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos de folios cobrados en la factura {LaFactura.FacturaNum}, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task LeerFolios()
        {
            try
            {
                IEnumerable<Z200_Folio> res = new List<Z200_Folio>();
                FiltroFolio nfo = new()
                {
                    EmpresaId = EmpresaActiva.OrgId
                };
                if (LosFoliosAll != null && !LosFoliosAll.Any())
                
                    await ReadLosFolios.InvokeAsync(nfo);

                LosFolios = LosFoliosAll != null && LosFoliosAll.Any(x => x.Status == true && x.Estado == 2 &&
                    x.EmpresaId == LaFactura.EmpresaId && x.OrgId == LaFactura.OrgId) ?
                                LosFoliosAll.Where(x => x.Status == true && x.Estado == 2 &&
                                x.EmpresaId == LaFactura.EmpresaId && x.OrgId == LaFactura.OrgId).ToList() :
                                new List<Z200_Folio>();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer PRODUCTOS, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task<ApiRespuesta<Z222_FactDet>> Servicio(string tipo, Z222_FactDet det)
        {
            ApiRespuesta<Z222_FactDet> resp = new()
            {
                Exito = false,
                Data = det
            };

            try
            {
                if (det != null)
                {
                    det.FacturaId = LaFactura.FacturaId;
                    if (tipo == "Insert")
                    {
                        det.FactDetId = Guid.NewGuid().ToString();
                        det.Estado = 1;
                        Z222_FactDet detInsert = await DetRepo.Insert(det);
                        if (detInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = detInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro de detalle de la factura {LaFactura.FacturaNum}");
                        }
                        return resp;
                    }
                    else if (tipo == "Update")
                    {
                        Z222_FactDet detUpdate = await DetRepo.Update(det);
                        if (detUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = detUpdate;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Actualizo el registro DETALLE DE FATURA {LaFactura.FacturaNum}");
                            resp.Exito = false;
                        }
                        return resp;
                    }
                    else if (tipo == "Importe")
                    {
                        Z200_Folio folioTmp = LosFolios.Any(x => x.FolioId == det.FolioId) ?
                            LosFolios.FirstOrDefault(x => x.FolioId == det.FolioId)! : new();
                        if (folioTmp == null || folioTmp.FolioId.Length < 30)
                        {
                            resp.Exito = false;
                            resp.Data = det;
                            return resp;
                        }
                        LaFactura.Control += det.Status ? folioTmp.Importe : (folioTmp.Importe * -1);
                        Z220_Factura factTmp = await FacturaRepo.Update(LaFactura);
                        if (factTmp != null)
                        {
                            resp.Exito = true;
                            resp.Data = det;
                        }
                        else
                        {
                            resp.Exito = false;
                            resp.Data = det;
                            resp.MsnError.Add("No pudo actualizarse el importe");
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
                await LogRepo.Insert(LogT);
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
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }

        }
        #endregion

    }
}

