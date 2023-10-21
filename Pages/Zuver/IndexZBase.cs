using System;
using System.Security.Claims;
using DashBoard.Data;
using DashBoard.Modelos;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Radzen;

namespace DashBoard.Pages.Zuver
{
	public class IndexZBase : ComponentBase
	{
        protected const string TBita = "Index Administracion";
        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgRepo { get; set; } = default!;
        [Inject]
        public Repo<Z110_User, ApplicationDbContext> UsersRepo { get; set; } = default!;
        [Inject]
        public Repo<ZConfig, ApplicationDbContext> ConfRepo { get; set; } = default!;

        
        public List<Z110_User> LosUsersAll { get; set; } = new List<Z110_User>();
        public List<Z100_Org> LasOrgAll { get; set; } = new List<Z100_Org>();
        public List<Z100_Org> LosClientesAll { get; set; } = new List<Z100_Org>();
        public List<Z100_Org> LasAlijadorasAll { get; set; } = new List<Z100_Org>();
        public Dictionary<string, string> DicDataAll { get; set; } = new Dictionary<string, string>();

        public Z100_Org EmpresaActivaAll { get; set; } = new();
        
        protected string CorporativoAll { get; set; } = "All";
        protected List<KeyValuePair<string, string>> TipoOrgsAll { get; set; } =
            new List<KeyValuePair<string, string>>();
        protected List<KeyValuePair<int, string>> NivelesAll { get; set; } =
            new List<KeyValuePair<int, string>>();

        bool PrimeraVez = true;
        protected override async Task OnInitializedAsync()
        {
            if (PrimeraVez)
            {
                PrimeraVez = false;
                await LeerElUser();
            }
        }

        public async Task Leer()
        {
            try
            {
                await LeerTiposAll();
                await LeerNiveles();
                await LeerOrgAll(); // Aqui mismo se lee LosClientesAll()

                await LeerUsersAll();
                await LeerEmpActiva();
                
    
                //await LeerNrpsAll();
                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                     $"Consulto la seccion de {TBita}", CorporativoAll, ElUser.OrgId);
                await BitacoraAll(bitaTemp);
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                     $"Error al entrar a la funcion LEER de {TBita}, {ex}",
                     CorporativoAll, ElUser.OrgId);
                await LogAll(logTemp);
            }
        }
        public async Task LeerUsersAll()
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
                LosUsersAll = resp.Any() ? resp.ToList() : LosUsersAll;
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error, No fue posible leer los usuarios {TBita}, {ex}",
                        "All", ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        public async Task LeerOrgAll()
        {
            try
            {
                IEnumerable<Z100_Org> res = new List<Z100_Org>();
                if (ElUser.Nivel < 5)
                {
                    res = await OrgRepo.Get(x => x.Estado == 1 && x.Corporativo == CorporativoAll);
                }
                else
                {
                    res = await OrgRepo.GetAll();
                }

                LasOrgAll = res.Any() ? res.ToList() : LasOrgAll;

                LosClientesAll = res.Any(x => x.Tipo == "Cliente") ?
                    res.Where(x => x.Tipo == "Cliente").ToList() : LosClientesAll;

                LasAlijadorasAll = res.Where(x => x.Tipo == "Administracion" && x.Estado == 1 &&
                                x.Status == true).ToList();
                
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error No fue posible leer las organizaciones, {TBita}, {ex}",
                    CorporativoAll, ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        public async Task LeerTiposAll()
        {
            try
            {
                if (!TipoOrgsAll.Any())
                {
                    string[] OrgT = Constantes.OrgTipo.Split(",");
                    foreach (var tc in OrgT)
                    {
                        if (tc == "Administracion" && ElUser.Nivel < 6)
                            continue;
                        TipoOrgsAll.Add(new KeyValuePair<string, string>(tc, tc));
                    }
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

        public async Task LeerNiveles()
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

        public async Task LeerEmpActiva()
        {
            try
            {
                IEnumerable<ZConfig> resp = await ConfRepo.Get(x => x.Usuario == ElUser.UserId);

                ZConfig? reg = resp.Any(x => x.Usuario == ElUser.UserId) ? resp.Where(x => x.Usuario == ElUser.UserId)
                                    .OrderByDescending(x => x.Fecha1).FirstOrDefault() : new();

                EmpresaActivaAll = reg != null ? LasOrgAll.FirstOrDefault(x => x.OrgId == reg.Txt)! : new();
            }
            catch (Exception ex)
            {
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error No fue posible leer la empresa ACTIVA, {TBita}, {ex}",
                    CorporativoAll, ElUser.OrgId);
                await LogAll(logTemp);
            }
        }

        #region Usuario y Bitacora

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
        public async Task BitacoraAll(Z190_Bitacora bita)
        {
            if (bita.Fecha.Subtract(LastBita.Fecha).TotalSeconds > 15 ||
                LastBita.Desc != bita.Desc || LastBita.Sistema != bita.Sistema ||
                LastBita.UserId != bita.UserId || LastBita.OrgId != bita.OrgId)
            {
                LastBita = bita;
                await BitaRepo.Insert(bita);
            }
        }
        public Z192_Logs LastLog { get; set; } = new();

        public async Task LogAll(Z192_Logs log)
        {
            if (LastLog.Desc != log.Desc || LastLog.Sistema != log.Sistema ||
                LastLog.UserId != log.UserId || LastLog.OrgId != log.OrgId)
            {
                LastLog = log;
                await LogRepo.Insert(log);
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
                    if (ElUser == null)
                    {
                        NM.NavigateTo("Identity/Account/Login?ReturnUrl=/", true);
                    }
                    
                    else if (ElUser.Nivel < 5)
                    {
                        
                        NM.NavigateTo("/indexc", true);
                    }
                    
                    else
                    { 
                        CorporativoAll = ElUser.OrgId;
                        await Leer();
                    }
                }
                else
                {
                    NM.NavigateTo("Identity/Account/Login?ReturnUrl=/", true);
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

