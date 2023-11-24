using System;
using DashBoard.Data;
using DashBoard.Modelos;
using MathNet.Numerics.Distributions;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Radzen;

namespace DashBoard.Pages.Web
{
	public class IndexClientesBase : ComponentBase 
	{
        protected const string TBita = "Index Clientes";

        [Inject]
        public Repo<ZConfig, ApplicationDbContext> ConfRepo { get; set; } = default!;
        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgRepo { get; set; } = default!;
        //[Inject]
        //public Repo<Z170_File, ApplicationDbContext> FileRepo { get; set; } = default!;
        
        [Inject]
        public Repo<Z200_Folio, ApplicationDbContext> FolioRepo { get; set; } = default!;
        [Inject]
        public Repo<Z220_Factura, ApplicationDbContext> FacturaRepo { get; set; } = default!;
        //[Inject]
        //public Repo<Z230_Pago, ApplicationDbContext> PagoRepo { get; set; } = default!;
        [Inject]
        public Repo<Z180_EmpActiva, ApplicationDbContext> EmpActRepo { get; set; } = default!;

        protected string CorporativoAll { get; set; } = "All";
        protected List<Z100_Org> LasOrgsAll { get; set; } = new List<Z100_Org>();
        protected List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        protected List<Z100_Org> LasAlijadorasAll { get; set; } = new List<Z100_Org>();
        protected List<Z110_User> LosUsersAll { get; set; } = new List<Z110_User>();
        // protected List<Z170_File> LosArchivosAll { get; set; } = new List<Z170_File>();
        protected List<Z180_EmpActiva> LasEmpActAll { get; set; } = new List<Z180_EmpActiva>();
        protected List<Z200_Folio> LosFoliosAll { get; set; } = new List<Z200_Folio>();
        protected List<Z220_Factura> LasFacturasAll { get; set; } = new List<Z220_Factura>();
        //protected List<Z230_Pago> LosPagosAll { get; set; } = new List<Z230_Pago>();
        protected List<ZConfig> LosConfigsAll { get; set; } = new List<ZConfig>();

        protected List<KeyValuePair<string, string>> TipoOrgsAll { get; set; } =
            new List<KeyValuePair<string, string>>();
        protected List<KeyValuePair<int, string>> NivelesAll { get; set; } =
            new List<KeyValuePair<int, string>>();

        protected int LasAdmonCANT { get; set; } = 0;
        protected Z100_Org EmpresaActivaAll { get; set; } = new();
        protected bool NohayEmpActiva { get; set; }
        protected bool PrimeraVez { get; set; } = true;
        protected override async Task OnInitializedAsync()
        {
            if (PrimeraVez)
            {
                PrimeraVez = false;
                if (ElUser == null || ElUser.UserId.Length < 15)
                    await LeerElUser();
                else
                    await Leer();
            }
        }

        protected async Task Leer()
        {
            try
            {
                await LeerOrgAll();

                if (LasOrgsAll.Any() && ElUser.UserId.Length > 30)
                    await LeerEmpresaActivaAll();


                await LeerTiposAll();
                await LeerNiveles();

                await LeerUsersAll();

                LasAdmonCANT = LasOrgsAll.Count(x => x.Tipo == "Administracion" &&
                                           x.Estado == 1 && x.Status == true);

                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                     $"Consulto la seccion de {TBita}", CorporativoAll, ElUser.OrgId);
                await BitacoraAll(bitaTemp);
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                     $"Error al entrar a la funcion LEER de {TBita}, {ex}", CorporativoAll, ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        protected async Task LeerEmpresaActivaAll()
        {
            try
            {
                LasEmpActAll = (await EmpActRepo.GetAll()).ToList();

                if (!LasEmpActAll.Any())
                {
                    NohayEmpActiva = true;
                    LasEmpActAll = new List<Z180_EmpActiva>();
                    EmpresaActivaAll = new();
                    return;
                }
                else
                {
                    string orgIdT = LasEmpActAll.Any(x => x.UserId == ElUser.UserId) ? LasEmpActAll.OrderByDescending(f => f.Fecha)
                        .FirstOrDefault(x => x.UserId == ElUser.UserId)!.OrgId :
                        "";

                    EmpresaActivaAll = orgIdT != "" ? LasOrgsAll.FirstOrDefault(x => x.OrgId == orgIdT)! :
                        new Z100_Org();
                }

                if (DateTime.Now.Hour < 10 && DateTime.Now.Day % 5 == 0 &&
                    LasEmpActAll.Count(x => x.UserId == ElUser.UserId) > 5)
                {
                    IEnumerable<Z180_EmpActiva> lista = LasEmpActAll.OrderByDescending(f => f.Fecha)
                                        .Where(x => x.UserId == ElUser.UserId).Skip(5);
                    if (lista.Any())
                    {
                        await EmpActRepo.DeletePlus(lista);
                    }
                }

            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error, No fue posible leer el historial de la empresas activas, {TBita}, {ex}",
                    "All", ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        protected async Task LeerOrgAll()
        {
            try
            {
                IEnumerable<Z100_Org> res = await OrgRepo.GetAll();

                if (res.Any())
                {
                    LasOrgs = ElUser.Nivel < 5 ? (res.Where(x => x.Estado == 1 && x.OrgId == CorporativoAll)).ToList() :
                                    new List<Z100_Org>();
                    LasOrgsAll = res.ToList();
                    LasAlijadorasAll = res.Where(x => x.Tipo == "Administracion" && x.Estado == 1 &&
                                x.Status == true).ToList();
                }

            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error No fue posible leer las organizaciones, {TBita}, {ex}",
                    CorporativoAll, ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        protected async Task LeerFacturas(FiltroFactura? ff)
        {
            try
            {
                IEnumerable<Z220_Factura> resp = new List<Z220_Factura>();
                if (ff == null || !ff.Datos)
                {
                    resp = await FacturaRepo.Get(x => x.Status == (ElUser.Nivel > 6 ? x.Status : true) &&
                                                      x.EmpresaId == EmpresaActivaAll.OrgId &&
                                                      x.OrgId == (ElUser.Nivel < 5 ? ElUser.OrgId : x.OrgId));
                }
                else
                {
                    resp = await FacturaRepo.Get(x => x.Status == (ElUser.Nivel > 6 ? x.Status : true) &&
                                                    x.EmpresaId == EmpresaActivaAll.OrgId &&
                                                    x.OrgId == ff.OrgId);
                }


                LasFacturasAll = resp.Any() ? resp.OrderByDescending(x => x.Fecha).ToList() :
                    new List<Z220_Factura>();
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error, No fue posible leer el historial de las facturas, {TBita}, {ex}",
                    "All", ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        protected async Task LeerFolios(FiltroFolio? ff)
        {
            try
            {
                IEnumerable<Z200_Folio> resp = new List<Z200_Folio>();
                if (ff == null || !ff.Datos)
                {
                    resp = await FolioRepo.Get(x => x.Status == (ElUser.Nivel > 6 ? x.Status : true) &&
                                                    x.EmpresaId == EmpresaActivaAll!.OrgId &&
                                                    x.OrgId == (ElUser.Nivel < 5 ? ElUser.OrgId : x.OrgId));
                }
                else
                {
                    resp = await FolioRepo.Get(x => x.Status == (ElUser.Nivel > 6 ? x.Status : true) &&
                                                    x.EmpresaId == EmpresaActivaAll!.OrgId &&
                                                    x.OrgId == ff.OrgId);
                }

                LosFoliosAll = resp.Any() ? resp.OrderByDescending(x => x.Fecha).ToList() : new List<Z200_Folio>();
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error, No fue posible leer los folios, {TBita}, {ex}",
                    "All", ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        protected async Task LeerUsersAll()
        {
            try
            {
                IEnumerable<Z110_User> resp = new List<Z110_User>();
                if (ElUser.Nivel < 5)
                {
                    resp = await UserRepo.Get(x => x.OrgId == ElUser.OrgId);
                }
                else
                {
                    resp = await UserRepo.GetAll();
                }
                LosUsersAll = resp.Any() ? resp.ToList() : new List<Z110_User>();
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error, No fue posible leer los usuarios {TBita}, {ex}", "All", ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        protected async Task LeerConfigAll()
        {
            try
            {
                IEnumerable<ZConfig> resp = await ConfRepo.GetAll();
                LosConfigsAll = resp.Any() ? resp.ToList() : new List<ZConfig>();
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error, No fue posible leer Zconfiguracion {TBita}, {ex}", "All", ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        protected async Task LeerTiposAll()
        {
            try
            {
                TipoOrgsAll.Clear();
                string[] OrgT = Constantes.OrgTipo.Split(",");
                foreach (var tc in OrgT)
                {
                    if (tc == "Administracion" && ElUser.Nivel < 6)
                        continue;
                    TipoOrgsAll.Add(new KeyValuePair<string, string>(tc, tc));
                }

            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error, No fue posible leer los TIPOS de organizaciones {TBita}, {ex}",
                    CorporativoAll, ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        protected async Task LeerNiveles()
        {
            try
            {
                if (NivelesAll.Count < 1)
                {
                    string[] NivelT = Constantes.Niveles.Split(",");

                    for (int i = 0; i < NivelT.Length; i++)
                    {
                        NivelesAll.Add(new KeyValuePair<int, string>(i + 1, NivelT[i]));
                    }
                }
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error, No fue posible leer los niveles de los usuarios, {TBita}, {ex}",
                    "All", ElUser.OrgId);
                await LogAll(logTemp);
            }
        }


        #region Usuario y Bitacora

        [CascadingParameter(Name = "CorporativoAll")]
        public string Corporativo { get; set; } = "All";

        [CascadingParameter(Name = "ElUserAll")]
        public Z110_User ElUser { get; set; } = new();

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
        [Inject]
        public Repo<Z110_User, ApplicationDbContext> UserRepo { get; set; } = default!;
        [Inject]
        public Repo<Z190_Bitacora, ApplicationDbContext> BitaRepo { get; set; } = default!;
        [Inject]
        public Repo<Z192_Logs, ApplicationDbContext> LogRepo { get; set; } = default!;

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
                await LogAll(LogT);
            }

        }

        [Inject]
        public UserManager<IdentityUser> UserMger { get; set; } = default!;

        [CascadingParameter]
        public Task<AuthenticationState> AuthStateTask { get; set; } = default!;

        public async Task LeerElUser()
        {
            try
            {
                ClaimsPrincipal user = (await AuthStateTask).User;
                if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
                {
                    string elId = UserMger.GetUserId(user) ?? "Vacio";
                    if (elId == null || elId == "Vacio") NM.NavigateTo("Identity/Account/Login?ReturnUrl=/", true);
                    ElUser = await UserRepo.GetById(elId!);

                    //Niveles = "1Registrado,2Proveedor,3Cliente,4Cliente_Admin,5Alija,6Alija_Admin";
                    if (ElUser == null || ElUser.UserId.Length < 15)
                    {
                        NM.NavigateTo("Identity/Account/Login?ReturnUrl=/clientes", true);
                    }

                    else
                    {
                        CorporativoAll = ElUser.OrgId;
                        
                        await Leer();
                    }
                }
                else
                {
                    NM.NavigateTo("Identity/Account/Login?ReturnUrl=/clientes", true);
                }

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                 $" Error al intentar leer EL USER USUARIO {TBita}, {ex}", "All", ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        #endregion

    }
}

