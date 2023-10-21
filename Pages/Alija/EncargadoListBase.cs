using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Alija
{
	public class EncargadoListBase : ComponentBase
	{
        public const string TBita = "Encargado";

        [Inject]
        public Repo<Z204_Empleado, ApplicationDbContext> EncargadoRepo { get; set; } = default!;

        public List<Z204_Empleado> LosEncargados { get; set; } = new List<Z204_Empleado>();

        [CascadingParameter(Name = "ElFolioAll")]
        public Z200_Folio ElFolio { get; set; } = new();
        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        public RadzenDataGrid<Z204_Empleado>? EncargadoGrid { get; set; } =
            new RadzenDataGrid<Z204_Empleado>();

        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                await LeerEncargados();
            }

            await Leer();
        }

        protected async Task Leer()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task LeerEncargados()
        {
            try
            {
                IEnumerable<Z204_Empleado> resp = new List<Z204_Empleado>();
                if (ElFolio != null && ElFolio.FolioId.Length > 30)
                {
                    resp = await EncargadoRepo.Get(x => x.FolioId == ElFolio.FolioId);
                }
                LosEncargados = resp != null && resp.Any() ? resp.ToList() : LosEncargados;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos del Encargados folio {ElFolio.FolioNum}, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task<ApiRespuesta<Z204_Empleado>> Servicio(string tipo, Z204_Empleado encargado)
        {
            ApiRespuesta<Z204_Empleado> resp = new()
            {
                Exito = false,
                Data = encargado
            };

            try
            {
                if (encargado != null)
                {
                    encargado.FolioId = ElFolio.FolioId;
                    if (tipo == "Insert")
                    {
                        encargado.EmpleadoId = Guid.NewGuid().ToString();

                        Z204_Empleado encargadoInsert = await EncargadoRepo.Insert(encargado);
                        if (encargadoInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = encargadoInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro de Encargado del folio {ElFolio.FolioNum}");
                        }
                        return resp;
                    }
                    else if (tipo == "Update")
                    {
                        Z204_Empleado EncargadoUpdate = await EncargadoRepo.Update(encargado);
                        if (EncargadoUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = EncargadoUpdate;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Actualizo el registro Encargado {ElFolio.FolioNum}");
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

