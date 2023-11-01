using System;
using DashBoard.Data;
using DashBoard.Modelos;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;

namespace DashBoard.Pages.Sistema
{
    public class FilesUpBase : ComponentBase
    {
        public const string TBita = "Subir Archivos";

        [Inject]
        public Repo<Z170_File, ApplicationDbContext> FileRepo { get; set; } = default!;

        [CascadingParameter(Name = "ElFolioAll")]
        public Z200_Folio ElFolio { get; set; } = new();
        [CascadingParameter(Name = "LaFacturaAll")]
        public Z220_Factura LaFactura { get; set; } = new();
        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        [Parameter]
        public string TipoArchivo { get; set; } = "";
        [Parameter]
        public bool FolioFactura { get; set; } = true;

        public string dropClass = string.Empty;
        public string ImageUrl = "";
        public bool Uploading = false;
        public string ElMensaje = "";
        public long UploadedBytes;
        public long TotalBytes;
        public List<string> FilesUrls = new List<string>();

        public void HandleDragEnter()
        {
            dropClass = "dropAreaDrug";
        }
        public void HandleDragLeave()
        {
            dropClass = string.Empty;
        }
        protected override async Task OnInitializedAsync()
        {
            await ListaDeArchivos();

        }

        
        public async Task ListaDeArchivos()
        {
            FilesUrls.Clear();
            string ruta_base = Path.Combine(Environment.CurrentDirectory, "Archivos");

            if (!Directory.Exists(ruta_base))
            {
                Directory.CreateDirectory(ruta_base);
            }
            var files = Directory.GetFiles(ruta_base, "*.*");
            foreach (var fname in files)
            {
                var file = Path.GetFileName(fname);
                string url = $"{file}";
                FilesUrls.Add(url);
            }
            await InvokeAsync(StateHasChanged);
        }
        

        public async Task OnInputFileChange(InputFileChangeEventArgs args)
        {
            string terminacion = Path.GetExtension(args.File.Name).ToLower();
            if (!TipoArchivoPermitido(args.File.Name))
            {
                ElMensaje = $"Tipo de archivo no permitido tu archivo es {terminacion} y esperamos un archivo {TipoArchivo}";
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar subir un archivo no valido {TBita} {ElMensaje}",
                        Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                return;
            }
            dropClass = string.Empty;
            UploadedBytes = 0;

            //long maxSizeBytes = 1048576; // 1 MB, puedes ajustar este valor según tus necesidades
            long maxSizeBytes = 2100000; // 2 MB, puedes ajustar este valor según tus necesidades

            if (args.File.Size > maxSizeBytes)
            {
                ElMensaje = $"El archivo excede el tamaño máximo permitido (1 MB).";
                return;
            }
            try
            {
                Uploading = true;
                await InvokeAsync(StateHasChanged);

                TotalBytes = args.File.Size;
                long percent = 0;
                long chunkSize = 400000;
                long numChunks = TotalBytes / chunkSize;
                long remainder = TotalBytes % chunkSize;

                string justFileName = Path.GetFileNameWithoutExtension(args.File.Name);
                string ff = FolioFactura ? "Folio" + ElFolio.FolioNum : "Factura" + LaFactura.FacturaNum;
                string newFileNameWOP = $"{justFileName}-{ff}-{DateTime.Now.Ticks.ToString()}{terminacion}";
                ApiRespValor hayCarpeta = ExisteCarpeta();
                if (!hayCarpeta.Exito)
                {
                    ElMensaje = $"No hay carpeta para guardar el archivo en el servidor";
                    Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar leer la carpeta de archivos y mes no existe {TBita} {ElMensaje}",
                        Corporativo, ElUser.OrgId);
                    await LogAll(LogT);
                    return;
                }
                string fileName = Path.Combine(hayCarpeta.Texto,newFileNameWOP);

                using (var inStream = args.File.OpenReadStream(long.MaxValue))
                {
                    using (var outStream = File.OpenWrite(fileName))
                    {
                        for (int i = 0; i < numChunks; i++)
                        {
                            var buffer = new byte[chunkSize];
                            await inStream.ReadAsync(buffer, 0, buffer.Length);
                            await outStream.WriteAsync(buffer, 0, buffer.Length);
                            UploadedBytes += chunkSize;
                            percent = UploadedBytes * 100 / TotalBytes;
                            ElMensaje = $"Subiendo {args.File.Name} {percent}%";
                            await InvokeAsync(StateHasChanged);
                        }
                        if (remainder > 0)
                        {
                            var buffer = new byte[remainder];
                            await inStream.ReadAsync(buffer, 0, buffer.Length);
                            await outStream.WriteAsync(buffer, 0, buffer.Length);
                            UploadedBytes += remainder;
                            percent = UploadedBytes * 100 / TotalBytes;
                            ElMensaje = $"Subiendo {args.File.Name} {percent}%";
                            await InvokeAsync(StateHasChanged);
                        }
                    }
                }

                ElMensaje = "Carga completa!";
                await Servicio(ServiciosTipos.Insert, newFileNameWOP, hayCarpeta.Texto);
                await ListaDeArchivos();
                Uploading = false;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar leer y crea carpeta de archivos {TBita} {ex}",
                        Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        private async Task<ApiRespuesta<Z170_File>> Servicio(ServiciosTipos tipo, string archivo, string folder)
        {
            ApiRespuesta<Z170_File> resp = new() { Exito = false };
            Z170_File registro = new()
            {
                FileId = Guid.NewGuid().ToString(),
                Fecha = DateTime.Now,
                Tipo = TipoArchivo,
                Folder = folder,
                Archivo = archivo,
                
                Estado = 1,

            };
            if (FolioFactura)
            {
                registro.FolioId = ElFolio.FolioId;
                registro.OrgId = ElFolio.OrgId;
                registro.EmpresaActiva = ElFolio.EmpresaId;
            }
            else
            {
                registro.FacturaId = LaFactura.FacturaId;
                registro.OrgId = LaFactura.OrgId;
                registro.EmpresaActiva = LaFactura.EmpresaId;
            }

            try
            {

                if (tipo == ServiciosTipos.Insert)
                {
                    Z170_File fileUped = await FileRepo.Insert(registro);
                    if (fileUped != null)
                    {
                        resp.Exito = true;
                        resp.Data = fileUped;
                    }
                }
                if (tipo == ServiciosTipos.Update)
                {

                }
            }
            catch (Exception ex)
            {
                resp.MsnError.Add( $"No fue posible actualizar el registro de archivos subidos al servidor, {ex}");
            }
            return resp;
        } 

        private static ApiRespValor ExisteCarpeta()
        {
            ApiRespValor respuesta = new() { Exito = false };
            try
            {
                string ruta_base = Path.Combine(Environment.CurrentDirectory, "Archivos");
                
                if (!Directory.Exists(ruta_base))
                {
                    Directory.CreateDirectory(ruta_base);
                }

                // Combina la carpeta base con el nombre de la carpeta
                DateTime fechaActual = DateTime.Now;
                string nombreCarpeta = fechaActual.ToString("MMyy");
                string carpetaDestino = Path.Combine(ruta_base, nombreCarpeta);

                // Verifica si la carpeta ya existe, si no, créala
                if (!Directory.Exists(carpetaDestino))
                {
                    Directory.CreateDirectory(carpetaDestino);
                }
                else
                {
                    bool rp = Directory.Exists(carpetaDestino);
                    Console.WriteLine(rp);
                }
                

                respuesta.Texto = carpetaDestino;
                respuesta.Exito = true;
                // Combina la ruta completa con el nombre del archivo
                //string rutaCompleta = Path.Combine(carpetaDestino, archivo.Name);
            }
            catch (Exception ex)
            {
                respuesta.MsnError.Add(ex.Message);
                
            }
            return respuesta;
        
        }

        private bool TipoArchivoPermitido(string nombreArchivo)
        {
            string ext = Path.GetExtension(nombreArchivo).ToLower();
            TipoArchivo = ext;
            //return extension == $".{TipoArchivo.ToLower()}";
            return ext == ".pdf" || ext == ".xml" || ext == ".jpg" || ext == ".jpeg" || ext == ".png";
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

