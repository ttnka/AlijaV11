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
        
        [Parameter]
        public Dictionary<string, string> DicData { get; set; } = new Dictionary<string, string>();
        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        [Parameter]
        public List<Z100_Org> LosClientes { get; set; } = new List<Z100_Org>();
        [Parameter]
        public List<Z220_Factura> LasFacturas { get; set; } = new List<Z220_Factura>();

        [Parameter]
        public EventCallback ReadLasOrgsAll { get; set; }

        public IEnumerable<Z200_Folio> LosFolios { get; set; } = new List<Z200_Folio>();
        public Z100_Org EmpresaActivaAll { get; set; } = new();
        

        public RadzenDataGrid<Z220_Factura>? FacturaGrid { get; set; } = new RadzenDataGrid<Z220_Factura>();

        
        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (EmpresaActivaAll == null || EmpresaActivaAll.OrgId.Length < 15)
                    await EmpActiva();

            }
            if (!LasOrgs.Any())
                await ReadLasOrgsAll.InvokeAsync();
            await Leer();
        }

        protected async Task Leer()
        {
            try
            {
                await LeerClientes();
                //await LeerFolios();
                
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        
        protected async Task LeerFolios()
        {
            try
            {
                Leyendo = true;
                if (EmpresaActivaAll == null || EmpresaActivaAll.OrgId.Length < 15)
                    await EmpActiva();

                // busca folios por empresa para CLIENTES y Alijadores
                IEnumerable<Z200_Folio> resp = await FolioRepo.Get(x => x.EmpresaId == EmpresaActivaAll!.OrgId &&
                    x.OrgId == (ElUser.Nivel < 5 ? ElUser.OrgId : x.OrgId));

                if (resp.Any())
                    LosFolios = resp.OrderByDescending(x => x.Fecha);
                
                Leyendo = false;
                StateHasChanged();

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer LOS FOLIOS, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task<ApiRespuesta<Z100_Org>> EmpActiva()
        {
            Z100_Org OrgRespuesta = new();
            ApiRespuesta<Z100_Org> respuesta = new()
            {
                Exito = false,
            };
            try
            {
                IEnumerable<ZConfig> reg = await ConfRepo.Get(x => x.Titulo == Constantes.EmpresaActiva &&
                                x.Usuario == ElUser.UserId);
                if (reg == null && !reg!.Any())
                {
                    respuesta.MsnError.Add("No hay registros empresa activa para este usuario");
                    respuesta.Exito = false;
                }
                else
                {
                    string orgIdT = reg!.OrderByDescending(x => x.Fecha1).FirstOrDefault()!.Txt;
                    EmpresaActivaAll = LasOrgs.FirstOrDefault(x => x.OrgId == orgIdT)!;
                    respuesta.Exito = true;

                }
                respuesta.Data = EmpresaActivaAll;
                return respuesta;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer Los Clientes, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
                respuesta.MsnError.Add(ex.Message);
                return respuesta;
            }
        }

        protected async Task LeerClientes()
        {
            try
            {
                LosClientes = LasOrgs.Where(x => x.Tipo == "Cliente" && x.Status == true).ToList();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer Los Clientes, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected Z200_Folio PassToOldFolio(Z200_Folio oldFolio)
        {
            Z200_Folio resp = new();
            resp.FolioId = oldFolio.FolioId.ToString();
            resp.FolioNum = oldFolio.FolioNum.ToString();
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

        protected async Task<string> FolioTxt(Z200_Folio folio)
        {
            //FolioEstado = "Nuevo,Ejecutado,Facturado,Pagado,Cancelado";
            try
            {
                string resp = $"Folio: {folio.FolioId}, Fecha: {folio.Fecha} Empresa: ";
                resp += LasOrgs.Any(x => x.OrgId == folio.OrgId) ?
                    LasOrgs.FirstOrDefault(x => x.OrgId == folio.OrgId)!.ComercialRfc : "Sin empresa";
                resp += $"Titulo: {folio.Titulo}, Comentarios: {folio.Obs}";
                var FolioEdo = Constantes.FolioEstado.Split(",");
                resp += $"Estado: {FolioEdo[folio.Estado - 1]} Estatus: ";
                resp += folio.Status ? "Activo" : "Suspendido";
                return resp;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar generer TEXTO en funcion FolioTxt de la pagina {TBita}, {ex}",
                        Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
                return "Error";
            }
        }

        protected async Task<string> NextFolio()
        {
            try
            {
                IEnumerable<Z200_Folio> resp = await FolioRepo.GetAll();
                if (resp == null) return "Vacio";
                return (resp.Count() + 1).ToString();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Generar el Siguiente FOLIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
                return "Vacio";
            }
        }

        protected async Task<ApiRespuesta<Z200_Folio>> Servicio(string tipo, Z200_Folio folio)
        {
            ApiRespuesta<Z200_Folio> resp = new()
            {
                Exito = false,
                Data = folio
            };

            try
            {
                if (folio != null)
                {
                    if (EmpresaActivaAll == null || EmpresaActivaAll.OrgId.Length < 15)
                    {
                        ApiRespuesta<Z100_Org> empAct = await EmpActiva();
                        if (!empAct.Exito)
                        {
                            resp.MsnError.Add("No hay empresa activa");
                            foreach (var e in empAct.MsnError)
                            {
                                resp.MsnError.Add(e);
                            }
                            return resp;
                        }
                    }

                    folio.EmpresaId = EmpresaActivaAll!.OrgId;
                    if (tipo == "Insert")
                    {
                        folio.FolioId = Guid.NewGuid().ToString();
                        folio.FolioNum = await NextFolio();
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
                    else if (tipo == "Update")
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
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }

        }
        #endregion

    }
}

