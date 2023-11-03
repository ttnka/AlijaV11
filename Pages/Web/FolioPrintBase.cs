using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Org.BouncyCastle.Math;
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
        public Repo<Z201_FolioPrint, ApplicationDbContext> PrintRepo { get; set; } = default!;
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
                    await Leer();

                }
            }

        }
        protected async Task Leer()
        {
            try
            {
                Z201_FolioPrint FPtemp = await PrintRepo.GetById(LlaveId);
                if (FPtemp == null || FPtemp.FolioId.Length != 36)
                {
                    LlaveId = "";
                    return;
                }
                else
                {
                    ElFolio = await FolioRepo.GetById(FPtemp.FolioId);
                    var resp = await CampoRepo.Get(x => x.FolioId == FPtemp.FolioId);
                    ElCampo = resp != null && resp.Any() ? resp.FirstOrDefault()! : new();
                }
                IEnumerable<ZConfig> gpoFolio = await ConfigRepo.Get(x => x.Usuario == ElFolio.EmpresaId &&
                                                                        x.Grupo == Constantes.GrupoFolio);

                if (gpoFolio.Any())
                {
                    string logoName = gpoFolio.Where(x=>x.Titulo == "Logotipo").OrderByDescending(x => x.Fecha1).FirstOrDefault().Txt;
                    ElLogo = Path.Join(Environment.CurrentDirectory, Constantes.FolderImagenes, logoName);

                    Eltitulo = gpoFolio.Where(x => x.Titulo == "TituloFolio").OrderByDescending(x => x.Fecha1).FirstOrDefault().Txt;
                    ElRecinto = gpoFolio.Where(x => x.Titulo == "Recinto").OrderByDescending(x => x.Fecha1).FirstOrDefault().Txt;
                    ElMail = gpoFolio.Where(x => x.Titulo == "Email").OrderByDescending(x => x.Fecha1).FirstOrDefault().Txt;
                    ElTel = gpoFolio.Where(x => x.Titulo == "Tel").OrderByDescending(x => x.Fecha1).FirstOrDefault().Txt;
                    ElCalce1 = gpoFolio.Where(x => x.Titulo == "Calce1").OrderByDescending(x => x.Fecha1).FirstOrDefault().Txt;
                    ElCalce2 = gpoFolio.Where(x => x.Titulo == "Calce2").OrderByDescending(x => x.Fecha1).FirstOrDefault().Txt;
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

