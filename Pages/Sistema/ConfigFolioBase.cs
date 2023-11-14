using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Sistema
{
	public class ConfigFolioBase : ComponentBase
	{
        protected const string TBita = "Configuracion de pagina de impresion de folios";
         

        [Inject]
        public Repo<ZConfig, ApplicationDbContext> ConfRepo { get; set; } = default!;

        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        //[Parameter]
        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();

        protected List<KeyValuePair<string, string>> LosTipos { get; set; } = new List<KeyValuePair<string, string>>();
        protected List<ZConfig> LosConfigs { get; set; } = new List<ZConfig>();
        protected List<Z100_Org> LasAdmin { get; set; } = new List<Z100_Org>();

        public RadzenDataGrid<ZConfig>? ConfigGrid { get; set; } = new RadzenDataGrid<ZConfig>();

        public string dropClass = string.Empty;
        public string ImageUrl = "";
        public bool Uploading = false;
        public string ElMensaje = "";
        public long UploadedBytes;
        public long TotalBytes;
        public string ElArchivo { get; set; } = "";

        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                PoblarTipos();
                
            }

            await Leer();
        }

        protected async Task Leer()
        {
            try
            {
                LosConfigs = (await ConfRepo.Get(x => x.Grupo == Constantes.GrupoFolio && x.Status == true)).ToList();

                LasAdmin = !LasOrgs.Any() ? new List<Z100_Org>() :
                            LasOrgs.Where(x => x.Tipo == "Administracion" && x.Estado == 1).ToList() ;

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
        protected void PoblarTipos()
        {
            if (LosTipos.Any()) LosTipos.Clear();

            LosTipos.Add(new KeyValuePair<string, string>("Logotipo", "Logotipo"));
            LosTipos.Add(new KeyValuePair<string, string>("TituloFolio", "Titulo del Folio"));
            LosTipos.Add(new KeyValuePair<string, string>("Recinto", "Recinto"));
            LosTipos.Add(new KeyValuePair<string, string>("Email", "Email"));
            LosTipos.Add(new KeyValuePair<string, string>("Tel", "Telefono"));
            LosTipos.Add(new KeyValuePair<string, string>("Calce1", "Calce Uno"));
            LosTipos.Add(new KeyValuePair<string, string>("Calce2", "Calce Dos"));
        }

        public void HandleDragEnter() => dropClass = "dropAreaDrug"; 
        public void HandleDragLeave() => dropClass = string.Empty;

        public async Task OnInputFileChange(InputFileChangeEventArgs args, ZConfig registro)
        {
            string terminacion = Path.GetExtension(args.File.Name).ToLower();
            if (!TipoArchivoPermitido(args.File.Name))
            {
                ElMensaje = $"Tipo de archivo no permitido tu archivo es {terminacion} ";
                ElMensaje += $"y esperamos un archivo tipo JPG, JPEG o PNG";
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

                
                ElArchivo = $"{justFileName}-Logo-{DateTime.Now.Ticks}{terminacion}";

                ApiRespValor hayCarpeta = ExisteCarpeta();

                if (!hayCarpeta.Exito)
                {
                    ElMensaje = $"No hay carpeta para guardar el archivo en el servidor";
                    Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar leer la carpeta de {Constantes.FolderImagenes} y mes no existe {TBita} {ElMensaje}",
                        Corporativo, ElUser.OrgId);
                    await LogAll(LogT);
                    return;
                }

                string fileName = Path.Combine(hayCarpeta.Texto, ElArchivo);

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

                registro.Txt = ElArchivo;
                ApiRespuesta<ZConfig> res = await Servicio(ServiciosTipos.Insert, registro);
                if (!res.Exito)
                {
                    throw new Exception("No fue posible agregar registro del logotipo");
                }
                else
                {
                    string t = "Se agrego un registro y un archivo para el logotipo de formato de Folio";
                    Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId, t,
                    Corporativo, ElUser.OrgId);
                    await BitacoraAll(bitaTemp);
                }
                
                Uploading = false;
                await ConfigGrid!.Reload();

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar leer y subir el archivo de logotipo {TBita} {ex}",
                        Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        private static ApiRespValor ExisteCarpeta()
        {
            ApiRespValor respuesta = new() { Exito = false };
            try
            {
                string ruta_base = Path.Combine("wwwroot", Constantes.FolderImagenes);
                for (var i = 0; i < 2; i++)
                {
                    if (!Directory.Exists(ruta_base))
                    {
                        Directory.CreateDirectory(ruta_base);
                    }
                    
                    ruta_base = i == 0 ? Path.Combine(ruta_base, "Web") : ruta_base;
                }

                respuesta.Texto = ruta_base;
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

        private static bool TipoArchivoPermitido(string nombreArchivo)
        {
            string ext = Path.GetExtension(nombreArchivo).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png";
        }

        protected async Task<ApiRespuesta<ZConfig>> Servicio(ServiciosTipos tipo, ZConfig config)
        {
            ApiRespuesta<ZConfig> resp = new()
            {
                Exito = false
            };

            try
            {
                if (config != null)
                {
                    config.Grupo = config.Grupo.ToUpper();
                    config.Tipo = config.Tipo.ToUpper();
                    if (tipo == ServiciosTipos.Insert)
                    {
                        config.ConfigId = Guid.NewGuid().ToString();
                        ZConfig configInsert = await ConfRepo.Insert(config);
                        if (configInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = configInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro {config.Titulo}");
                        }
                        return resp;
                    }
                    else if (tipo == ServiciosTipos.Update)
                    {
                        ZConfig configUpdate = await ConfRepo.Update(config);
                        if (configUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = configUpdate;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Actualizo el registro {config.Titulo}");
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

