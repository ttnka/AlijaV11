using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Sistema
{
	public class FileListBase : ComponentBase 
	{
        public const string TBita = "Lista de Archivos";

        [Inject]
        public Repo<Z170_File, ApplicationDbContext> FileRepo { get; set; } = default!;
        
        
        [CascadingParameter(Name = "ElFolioAll")]
        public Z200_Folio ElFolio { get; set; } = new();
        [CascadingParameter(Name = "LaFacturaAll")]
        public Z220_Factura LaFactura { get; set; } = new();
        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        public RadzenDataGrid<Z170_File>? FacturaGrid { get; set; } = new RadzenDataGrid<Z170_File>();

        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                // await LeerConceptos();
            }

            await Leer();
        }

        protected async Task Leer()
        {
            try
            {
                //await LeerProductos();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerFilesServidor()
        {
            try
            {
                IEnumerable<Z170_File> filesServ = await FileRepo.Get(x => x.Fecha > (DateTime.Now).AddDays(180) &&
                    x.Status == true); 
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer archivos en servidor {ElFolio.FolioNum}, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }
        

        protected async Task LeerFilesDB()
        {
            try
            {
                string path = Path.Combine(Environment.CurrentDirectory, "Archivos").ToString();
                //List<FileInfo> listaArchivos = await ListDirectoriesAndFiles(path);
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer archivos registrados en la base de datos, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerArchivosExistentes()
        {
            try
            {
                string path = Path.Combine(Environment.CurrentDirectory, "Archivos").ToString();
                List<FilesInfo> listaArchivos = await ListDirectoriesAndFiles(path);
                IEnumerable<Z170_File> filesServ = await FileRepo.Get(x => x.Fecha > DateTime.Now.AddDays(180) &&
                       x.FolioId == (ElFolio.FolioId.Length > 30 ? ElFolio.FolioId : x.FolioId) &&
                       x.FacturaId == (LaFactura.FacturaId.Length > 30 ? LaFactura.FacturaId : x.FacturaId) &&
                       x.Status == true);

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer archivos registrados en la base de datos, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }


        

        protected async Task<List<FilesInfo>> ListDirectoriesAndFiles(string directoryPath)
        {
            List<FilesInfo> items = new();
            try
            {
                ListDirectoriesAndFilesRecursive(directoryPath, items);
                return items;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer archivos registrados en el servidor, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
            return items;
        }

        private void ListDirectoriesAndFilesRecursive(string directoryPath, List<FilesInfo> items)
        {
            if (Directory.Exists(directoryPath))
            {
                string[] directories = Directory.GetDirectories(directoryPath);
                string[] files = Directory.GetFiles(directoryPath);

                foreach (var directory in directories)
                {
                    items.Add(new FilesInfo { Name = directory, IsDirectory = true });
                    ListDirectoriesAndFilesRecursive(directory, items);
                }

                foreach (var file in files)
                {
                    items.Add(new FilesInfo { Name = file, IsDirectory = false });
                }
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
                    if (ElFolio.FolioId.Length > 30)
                    {
                        archivo.FolioId = ElFolio.FolioId;
                    }
                    else if (LaFactura.FacturaId.Length > 30)
                    {
                        archivo.FacturaId = LaFactura.FacturaId;
                    }
                    else
                    {
                        return resp;
                    }
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

