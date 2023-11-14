using System;
using DashBoard.Data;
using DashBoard.Modelos;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;

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
        public bool FolioFactura { get; set; } = true;
        [Parameter]
        public EventCallback<string> ReadFileList { get; set; } 

        public Z170_File Borrador { get; set; } = new();

        public string ElFolder { get; set; } = "";
        public string ElArchivo { get; set; } = "";
        public string ElTipoArchivo { get; set; } = "";
        public string ElTitulo { get; set; } = "";

        public List<KeyValuePair<string, string>> DocsTipo { get; set; } = new List<KeyValuePair<string, string>>();

        public string dropClass = string.Empty;
        public string ImageUrl = "";
        public bool Uploading = false;
        public string ElMensaje = "";
        public long UploadedBytes;
        public long TotalBytes;

        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;
        
        protected override async Task OnInitializedAsync()
        {
            LeerDocsTipos();
            
        }

        public void HandleDragEnter() => dropClass = "dropAreaDrug";
        
        public void HandleDragLeave() => dropClass = string.Empty;

        public async Task OnInputFileChange(InputFileChangeEventArgs args)
        {
            string terminacion = Path.GetExtension(args.File.Name).ToLower();
            if (!TipoArchivoPermitido(args.File.Name))
            {
                ElMensaje = $"Tipo de archivo no permitido tu archivo es {terminacion} ";
                ElMensaje += $"y esperamos un archivo tipo {ElTipoArchivo}";
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar subir un archivo no valido {TBita} {ElMensaje}",
                        Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                return;
            }
            dropClass = string.Empty;
            UploadedBytes = 0;

            try
            {
                Uploading = true;
                await InvokeAsync(StateHasChanged);

                TotalBytes = args.File.Size;
                long percent = 0;
                long chunkSize = 400000;
                long numChunks = TotalBytes / chunkSize;
                long remainder = TotalBytes % chunkSize;

                string justFileName = Path.GetFileNameWithoutExtension(args.File.Name).ToLower();
                justFileName = justFileName.Replace(" ", "_");
                justFileName = justFileName.Replace(".", "_");

                string ff = FolioFactura ? "Folio_" + ElFolio.FolioNum : "Factura_" + LaFactura.FacturaNum;
                ElArchivo = $"{justFileName}-{ff}-{DateTime.Now.Ticks}{terminacion}";

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
                ElFolder = hayCarpeta.Texto;
                string fileName = Path.Combine(ElFolder, ElArchivo);

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

                string t = $"registro de archivo {ElFolder} {ElArchivo} y subir el archivo titulo";
                t += FolioFactura ? $"Folio {ElFolio.FolioNum} " : $"Factura {LaFactura.FacturaNum}";
                t += $"Fecha: {DateTime.Now}, Empresa Act{EmpresaActiva.Comercial}";

                ApiRespuesta<Z170_File> res = await Servicio(ServiciosTipos.Insert);
                if (!res.Exito)
                {
                    t = "No fue posible agregar " + t;   
                    throw new Exception(t);
                }
                else
                {
                    t = "Se agrego un ";
                    Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId, t,
                    Corporativo, ElUser.OrgId);
                    await BitacoraAll(bitaTemp);
                }
                Borrador = new();
                Borrador.Tipo = "";
                await ReadFileList.InvokeAsync(ElFolio.FolioId);
                Uploading = false;
                StateHasChanged();

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar leer y crea carpeta de archivos {TBita} {ex}",
                        Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task<ApiRespuesta<Z170_File>> Servicio(ServiciosTipos tipo)
        {
            ApiRespuesta<Z170_File> resp = new() { Exito = false };
            Z170_File registro = new()
            {
                FileId = Guid.NewGuid().ToString(),
                Fecha = DateTime.Now,
                Tipo = ElTipoArchivo,
                Folder = ElFolder,
                Archivo = ElArchivo,
                Titulo = ElTitulo,
                EmpresaActiva = EmpresaActiva.OrgId,
                Estado = 1,

            };
            if (FolioFactura)
            {
                registro.FolioId = ElFolio.FolioId;
                registro.OrgId = ElFolio.OrgId;
            }
            else
            {
                registro.FacturaId = LaFactura.FacturaId;
                registro.OrgId = LaFactura.OrgId;
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

                    // no hay update
                }
            }
            catch (Exception ex)
            {
                resp.MsnError.Add( $"No fue posible actualizar el registro de archivos subidos al servidor, {ex}");
            }
            return resp;
        } 

        private ApiRespValor ExisteCarpeta()
        {
            ApiRespValor respuesta = new() { Exito = false };
            try
            {
                string ruta_base = Path.Combine("wwwroot", Constantes.FolderImagenes);

                for (var i = 0; i < 3; i++)
                {
                    if (!Directory.Exists(ruta_base))
                    {
                        Directory.CreateDirectory(ruta_base);
                    }

                    if (i == 2) continue;
                    ruta_base = i == 0 ? Path.Combine(ruta_base, "Archivos") : Path.Combine(ruta_base, ElFolio.Fecha.ToString("MMyy"));
                }

                respuesta.Texto = ruta_base;
                    //Path.Join(Constantes.FolderImagenes, "Archivos", ElFolio.Fecha.ToString("MMyy"));
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

            if (FolioFactura)
            {
                return ext == ".jpg" || ext == ".jpeg" || ext == ".png";
            }
            return ext == ".pdf" || ext == ".xml";
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

