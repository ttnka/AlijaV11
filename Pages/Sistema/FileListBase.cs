using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Sistema
{
	public class FileListBase : ComponentBase 
	{
        public const string TBita = "Lista de Archivos";

        [Inject]
        public Repo<Z170_File, ApplicationDbContext> FileRepo { get; set; } = default!;

        [Parameter]
        public EventCallback<string> ReadFilesList { get; set; }
        
        
        [CascadingParameter(Name = "ElFolioAll")]
        public Z200_Folio ElFolio { get; set; } = new();
        [CascadingParameter(Name = "LaFacturaAll")]
        public Z220_Factura LaFactura { get; set; } = new();
        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();           

        [Parameter]
        public bool FolioFactura { get; set; } = true;
        [Parameter]
        public List<Z170_File> LosArchivos { get; set; } = new List<Z170_File>();

        public List<KeyValuePair<string, string>> DocsTipo { get; set; } = new List<KeyValuePair<string, string>>();
        public Dictionary<string, string> DicData { get; set; } = new Dictionary<string, string>(); 
        

        public RadzenDataGrid<Z170_File>? FilesGrid { get; set; } = new RadzenDataGrid<Z170_File>();

        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                LeerDocsTipos(); 
            }
            AddDicData();
            await Leer();
        }

        protected async Task Leer()
        {
            try
            {
                if (!LosArchivos.Any())
                    await LeerArchivosExistentes();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer los archivos en el servidor, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerArchivosExistentes()
        {
            try
            {
                await ReadFilesList.InvokeAsync(ElFolio.FolioId);
                AddDicData();

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer archivos registrados en la base de datos y servidor, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }


        protected void AddDicData()
        {
            if (LosArchivos != null && LosArchivos.Any())
            {
                foreach (var f in LosArchivos)
                {
                    string comp = Path.Combine(f.Folder, f.Archivo);
                    Console.WriteLine($" D: {Directory.Exists(f.Folder)}, F: {File.Exists(comp)}"); // BORRAR ESTA LINEA
                    if (Directory.Exists(f.Folder) && File.Exists(comp) &&
                                        !DicData.ContainsKey($"FileId_{f.FileId}"))

                        DicData.Add($"FileId_{f.FileId}", f.Archivo);
                }
                
            }
        }

        protected void LeerDocsTipos()
        {
            if (FolioFactura)
            {
                DocsTipo.Add(new KeyValuePair<string, string>("Fotografia", "Fotografia"));
            }
            else
            {
                DocsTipo.Add(new KeyValuePair<string, string>("Factura Pdf", "Factura Pdf"));
                DocsTipo.Add(new KeyValuePair<string, string>("Factura Xls", "Factura Xls"));
                DocsTipo.Add(new KeyValuePair<string, string>("Confirmacion Pdf", "Confirmacion Pdf"));
            }

        }

        protected async Task<ApiRespuesta<Z170_File>> Servicio(ServiciosTipos tipo, Z170_File archivo)
        {
            ApiRespuesta<Z170_File> resp = new()
            {
                Exito = false,
                Data = archivo
            };
            try
            {
                if (archivo != null)
                {
                    
                    if (tipo == ServiciosTipos.Insert)
                    {
                        // no hay insert
                    }
                    else if (tipo == ServiciosTipos.Update)
                    {
                        // no hay update
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

