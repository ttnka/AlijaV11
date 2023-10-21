using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Alija
{
	public class ConceptoListBase : ComponentBase 
	{
        public const string TBita = "Conceptos";

        [Inject]
        public Repo<Z210_Concepto, ApplicationDbContext> ConceptoRepo { get; set; } = default!;
        [Inject]
        public Repo<Z280_Producto, ApplicationDbContext> ProductoRepo { get; set; } = default!;

        public List<Z210_Concepto> LosConceptos { get; set; } = new List<Z210_Concepto>();
        public List<Z280_Producto> LosProductos { get; set; } = new List<Z280_Producto>();

        [CascadingParameter(Name = "ElFolioAll")]
        public Z200_Folio ElFolio { get; set; } = new();
        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        public RadzenDataGrid<Z210_Concepto>? ConceptoGrid { get; set; } = new RadzenDataGrid<Z210_Concepto>();

        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                await LeerConceptos();
            }

            await Leer();
        }

        protected async Task Leer()
        {
            try
            {
                await LeerProductos();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task LeerConceptos()
        {
            try
            {
                IEnumerable<Z210_Concepto> resp = new List<Z210_Concepto>();
                if (ElFolio != null && ElFolio.FolioId.Length > 30)
                {
                    resp = await ConceptoRepo.Get(x => x.FolioId == ElFolio.FolioId);
                }
                LosConceptos = resp != null && resp.Any() ? resp.ToList() : LosConceptos;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos de los conceptos de este folio {ElFolio.FolioNum}, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task LeerProductos()
        {
            try
            {
                IEnumerable<Z280_Producto> resp = new List<Z280_Producto>();
                if (ElUser.Nivel == 6)
                {
                    resp = await ProductoRepo.GetAll();
                }
                else
                {
                    resp = await ProductoRepo.Get(x => x.Estado == 1 && x.Status == true);
                }
                LosProductos = resp != null && resp.Any() ? resp.ToList() : LosProductos; 
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer PRODUCTOS, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task<ApiRespuesta<Z210_Concepto>> Servicio(string tipo, Z210_Concepto concepto)
        {
            ApiRespuesta<Z210_Concepto> resp = new()
            {
                Exito = false,
                Data = concepto
            };

            try
            {
                if (concepto != null)
                {
                    concepto.FolioId = ElFolio.FolioId;
                    if (tipo == "Insert")
                    {
                        concepto.ConceptoId = Guid.NewGuid().ToString();

                        Z210_Concepto conceptoInsert = await ConceptoRepo.Insert(concepto);
                        if (conceptoInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = conceptoInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro de Concepto del folio {ElFolio.FolioNum}");
                        }
                        return resp;
                    }
                    else if (tipo == "Update")
                    {
                        Z210_Concepto conceptoUpdate = await ConceptoRepo.Update(concepto);
                        if (conceptoUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = conceptoUpdate;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Actualizo el registro CONCEPTO {ElFolio.FolioNum}");
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

