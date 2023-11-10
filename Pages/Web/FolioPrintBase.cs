using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualStudio.Web.CodeGeneration;
using QRCoder;

using Radzen;

namespace DashBoard.Pages.Web
{
    public class FolioPrintBase : ComponentBase 
	{
        public const string TBita = "Folios impresion";

        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgRepo { get; set; } = default!;
        [Inject]
        public Repo<Z200_Folio, ApplicationDbContext> FolioRepo { get; set; } = default!;
        
        [Inject]
        public Repo<Z209_Campos, ApplicationDbContext> CampoRepo { get; set; } = default!;
        [Inject]
        public Repo<ZConfig, ApplicationDbContext> ConfigRepo { get; set; } = default!;


        [Parameter]
		public string LlaveId { get; set; } = "";

        protected Z200_Folio ElFolio { get; set; } = new();
        protected Z209_Campos ElCampo { get; set; } = new();

        protected Dictionary<string, string> DicData { get; set; } = new Dictionary<string, string>();
        protected bool SinLlave { get; set; } = false;
        protected bool Primera { get; set; } = true;

        protected string ElLogo { get; set; } = "";
        protected string Eltitulo { get; set; } = "";
        protected string ElRecinto { get; set; } = "";
        protected string ElMail { get; set; } = "";
        protected string ElTel { get; set; } = "";
        protected string ElCalce1 { get; set; } = "";
        protected string ElCalce2 { get; set; } = "";
        protected Z100_Org LaEmpAct { get; set; } = new();
        protected Z100_Org ElCliente { get; set; } = new();

        protected string LaUrl { get; set; } = "";
        protected string ElQr { get; set; } = "";
        

        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (LlaveId == "" || LlaveId.Length != 36)
                {
                    SinLlave = true;
                }
                else
                {
                    LaUrl = $"{Constantes.ElDominio}/folio/{LlaveId}";
                    await Leer();

                    await GenerarQr(ElFolio.Fecha);
                }
            }

        }
        protected async Task Leer()
        {
            try
            {
                ElFolio = await FolioRepo.GetById(LlaveId);
                var resp = await CampoRepo.Get(x => x.FolioId == LlaveId);
                ElCampo = resp != null && resp.Any() ? resp.FirstOrDefault()! : new();
                
                IEnumerable<ZConfig> gpoFolio = await ConfigRepo.Get(x => x.Usuario == ElFolio.EmpresaId &&
                                                                        x.Grupo == Constantes.GrupoFolio);
                if (gpoFolio.Any())
                {
                    // busca el logotipo de la empresa que se imprime, en caso de no encontrar
                    // usa el logo por default Logo1.png
                    string logoName = !gpoFolio.Any(x => x.Titulo == "Logotipo") ? "Logo1.png" :
                        gpoFolio.Where(x=>x.Titulo == "Logotipo").OrderByDescending(x => x.Fecha1).FirstOrDefault()!.Txt!;
                    
                    ElLogo = Path.Join(Constantes.FolderImagenes, "Web", logoName);

                    Eltitulo = !gpoFolio.Any(x => x.Titulo == "TituloFolio") ? "" :
                        gpoFolio.Where(x => x.Titulo == "TituloFolio").OrderByDescending(x => x.Fecha1).FirstOrDefault()!.Txt!;
                    ElRecinto = !gpoFolio.Any(x => x.Titulo == "Recinto") ? "" :
                        gpoFolio.Where(x => x.Titulo == "Recinto").OrderByDescending(x => x.Fecha1).FirstOrDefault()!.Txt!;
                    ElMail = !gpoFolio.Any(x => x.Titulo == "Email") ? "" :
                        gpoFolio.Where(x => x.Titulo == "Email").OrderByDescending(x => x.Fecha1).FirstOrDefault()!.Txt!;
                    ElTel = !gpoFolio.Any(x => x.Titulo == "Tel") ? "" :
                        gpoFolio.Where(x => x.Titulo == "Tel").OrderByDescending(x => x.Fecha1).FirstOrDefault()!.Txt!;
                    ElCalce1 = !gpoFolio.Any(x => x.Titulo == "Calce1") ? "" :
                        gpoFolio.Where(x => x.Titulo == "Calce1").OrderByDescending(x => x.Fecha1).FirstOrDefault()!.Txt!;
                    ElCalce2 = !gpoFolio.Any(x => x.Titulo == "Calce2") ? "" :
                        gpoFolio.Where(x => x.Titulo == "Calce2").OrderByDescending(x => x.Fecha1).FirstOrDefault()!.Txt!;
                }

                IEnumerable<ZConfig> dontshow = await ConfigRepo.Get(x => x.SiNo == false && x.Grupo == "CAMPOS" &&
                                            x.Tipo == "MOSTRADOS");
                if (dontshow != null && dontshow.Any())
                {
                    foreach (var c in dontshow)
                    {
                        if (!DicData.ContainsKey(c.Titulo))
                            DicData.Add(c.Titulo, c.Titulo);
                    }
                }

                IEnumerable<Z100_Org> orgTmp = await OrgRepo.Get(x => x.OrgId == ElFolio.EmpresaId || x.OrgId == ElFolio.OrgId);
                LaEmpAct = orgTmp != null && orgTmp.Any(x=>x.OrgId == ElFolio.EmpresaId) ?
                    orgTmp.Where(x => x.OrgId == ElFolio.EmpresaId).FirstOrDefault()! : new();
                ElCliente = orgTmp != null && orgTmp.Any(x => x.OrgId == ElFolio.OrgId) ?
                    orgTmp.Where(x => x.OrgId == ElFolio.OrgId).FirstOrDefault()! : new();

                string txt = LlaveId == "" ? "No consulto la pagina correcta" : $"Se consulto el folio {ElFolio.FolioNum}";
                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora("Usuario Anonimo", "Usuario Anonimo",
                     $"Consulto {TBita} {txt}", "Usuario Anonimo", "Usuario Anonimo");
                await BitacoraAll(bitaTemp);

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog("Usuario Anonimo", "Usuario Anonimo",
                   $"Error al intentar LEER un FOLIO , {TBita},{ex}",
                   "Usuario Anonimo", "Usuario Anonimo");
                await LogAll(LogT);
            }
        }
        
        protected ApiRespValor ExisteFolder(DateTime fecha)
        {
            ApiRespValor resp = new() { Exito = false};
            try
            {
                // Verificar si el directorio "Imagenes" existe, si no, créalo.
                string folder = Path.Combine("wwwroot", Constantes.FolderImagenes);
                string carpetaMes = fecha.ToString("MMyy");
                for (var i = 0; i < 3; i++)
                {
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    if (i == 2) continue;
                    folder = i == 0 ? Path.Combine(folder, "Qr") : Path.Combine(folder, carpetaMes)  ;
                }
                resp.Exito = true;
                resp.Texto = Path.Join(Constantes.FolderImagenes, "Qr", carpetaMes);
            }
            catch (Exception ex)
            {
                resp.MsnError.Add($"Error la intentar generar las carpetas para guardar QR {ex.Message}");
            }
            return resp;
        }

        protected async Task GenerarQr(DateTime fecha)
        {
            try
            {
                ApiRespValor extFolder = ExisteFolder(fecha);

                if (!extFolder.Exito) throw new Exception(", no se genero el archivo QR, " + extFolder.MsnError);
                

                // Comprobar si el archivo de imagen QR ya existe.
                string nombreArchivo = Path.Join("wwwroot", extFolder.Texto, ElFolio.FolioId + ".png");
                if (!File.Exists(nombreArchivo))
                {
                    // El archivo de imagen QR no existe, generémoslo.
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrData = qrGenerator.CreateQrCode(LaUrl, QRCodeGenerator.ECCLevel.Q);
                    BitmapByteQRCode qrCode = new BitmapByteQRCode(qrData);
                    byte[] qrArrayBytes = qrCode.GetGraphic(20);

                    // Guardar la imagen generada en el directorio "Imagenes".
                    File.WriteAllBytes(nombreArchivo, qrArrayBytes);

                }

                // Configurar la URL de la imagen para mostrarla en tu página.
                 
                string nom = ElFolio.FolioId + ".png";
                ElQr = Path.Join("Imagenes", "Qr", fecha.ToString("MMyy"), nom);
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog("Usuario Anonimo", "Usuario Anonimo",
                    $"Error al intentar GENERAR o cargar QR , {TBita},{ex}",
                    "Usuario Anonimo", "Usuario Anonimo");
                await LogAll(LogT);
            }
        }


        #region Usuario y Bitacora

        [Inject]
        public Repo<Z190_Bitacora, ApplicationDbContext> BitaRepo { get; set; } = default!;
        [Inject]
        public Repo<Z192_Logs, ApplicationDbContext> LogRepo { get; set; } = default!;
        [Inject]
        public NavigationManager NM { get; set; } = default!;

        public MyFunc MyFunc { get; set; } = new MyFunc();
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
                Z192_Logs LogT = MyFunc.MakeLog("Usuario Anonimo", "Usuario Anonimo",
                    $"Error al intentar escribir BITACORA, {TBita},{ex}",
                    "Usuario Anonimo", "Usuario Anonimo");
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
                Z192_Logs LogT = MyFunc.MakeLog("Usuario Anonimo", "Usuario Anonimo",
                    $"Error al intentar escribir Log de Bitacora, {TBita},{ex}",
                    "Usuario Anonimo", "Usuario Anonimo");
                await LogRepo.Insert(LogT);
            }

        }
        #endregion

    }
}

