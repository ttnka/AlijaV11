using System;
using System.Security.Claims;
using System.Text.RegularExpressions;
using DashBoard.Data;
using DashBoard.Modelos;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.RootFinding;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Sistema
{
	public class BitacoraBase : ComponentBase
	{
        public const string TBita = "Bitacora";
        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgsRepo { get; set; } = default!;
        [Inject]
        public Repo<Z110_User, ApplicationDbContext> UserRepo { get; set; } = default!;
        public List<Z110_User> LosUsers { get; set; } = new List<Z110_User>();

        public List<Z190_Bitacora> LasBitas { get; set; } = new();
        public Filtro SearchBita { get; set; } = new();
        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new();
        public List<Z100_Org> OrgsFiltro { get; set; } = new();
        public List<Z100_Org> CorpsFiltro { get; set; } = new();
        public List<Z110_User> UsersFiltro { get; set; } = new();

        public List<Z192_Logs> LosLogs { get; set; } = new();

        public RadzenDataGrid<Z190_Bitacora>? BitaGrid { get; set; } =
            new RadzenDataGrid<Z190_Bitacora>();
        public RadzenDataGrid<Z192_Logs>? LogGrid { get; set; } =
            new RadzenDataGrid<Z192_Logs>();

        [CascadingParameter(Name = "CorportativoAll")]
        public string Corporativo { get; set; } = "All";

        public Dictionary<string, string> DicData { get; set; } =
            new Dictionary<string, string>();

        public string LaOrgName { get; set; } = "Todas las Organizaciones";

        protected bool Leyendo { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            if (ElUser == null || ElUser.UserId.Length < 15)
                await LeerElUser();
            else
                await Leer();
        }

        public async Task Leer()
        {
            try
            {
                await ReadOrg();
                await LeerUsers();
                await LeerBitacoras(SearchBita);
                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                    $"Consulto listado de bitacora, {TBita}", Corporativo, ElUser.OrgId);
                await BitaRepo.Insert(bitaTemp);

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar leer los valores iniciales funcion LEER, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        public async Task ReadOrg()
        {
            try
            {
                if (LasOrgs.Any())
                { 
                OrgsFiltro = ElUser.Nivel < 5 ?
                        LasOrgs.Where(x => x.OrgId == ElUser.OrgId)
                        .Select(x => x).ToList() :
                        LasOrgs.ToList();
                CorpsFiltro = LasOrgs.GroupBy(x => x.Corporativo)
                        .Select(x => x.First()).ToList();
                }
                /*
                var resultado = await OrgsRepo.GetAll();
                if (resultado.Any())
                {
                    OrgsFiltro = ElUser.Nivel < 5 ?
                        resultado.Where(x => x.OrgId == ElUser.OrgId)
                        .Select(x => x).ToList() :
                        resultado.ToList();

                    LasOrgs = resultado.ToList();

                    CorpsFiltro = resultado.GroupBy(x => x.Corporativo)
                        .Select(x => x.First()).ToList();
                }*/
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar leer organizaciones, {TBita},{ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }

        public async Task LeerUsers()
        {
            try
            {
                IEnumerable<Z110_User> resultado = await UserRepo.GetAll();
                if (resultado.Any())
                {
                    // Pobla los usuarios para el filtro
                    UsersFiltro = ElUser.Nivel < 5 ?
                        resultado.Where(x => x.OrgId == ElUser.OrgId)
                        .Select(x=>x).ToList() :
                        resultado.ToList();


                    // pobla todos los usuarios para los nombres de la lista
                    LosUsers = resultado.ToList();
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar leer los usuarios, {TBita},  {ex} de la bitacora",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
                LosUsers = new();
            }
        }

        protected async Task LeerBitacoras(Filtro datos)
        {            
            try
            {
                Leyendo = true;
                IEnumerable<Z190_Bitacora> resultado = new List<Z190_Bitacora>();
                if (datos.Datos)
                {
                    if (datos.Rango)
                    {
                        datos.FFin = datos.FIni > datos.FFin ?
                            datos.FIni.AddDays(1) : datos.FFin;
                    }
                    else
                    {
                        datos.FFin = datos.FIni.AddDays(1);
                    }
                    
                    resultado = await BitaRepo.Get(x => x.EmpresaId == ElUser.OrgId &&
                        x.Fecha >= datos.FIni &&
                        x.Fecha <= datos.FFin &&
                        x.UserId == (string.IsNullOrEmpty(datos.UserId) ?
                        x.UserId : datos.UserId) &&
                        x.OrgId == (string.IsNullOrEmpty(datos.OrgId) ?
                        x.OrgId : datos.OrgId) &&
                        x.Corporativo == (string.IsNullOrEmpty(Corporativo) ?
                        x.Corporativo : datos.Corporativo ) &&
                        x.Desc.Contains(string.IsNullOrEmpty(datos.Desc) ?
                        x.Desc : datos.Desc));
                }
                else
                {
                    resultado = await BitaRepo.Get(x => x.EmpresaId == ElUser.OrgId);
                }

                if (resultado == null || !resultado.Any())
                {
                    LasBitas.Clear();
                }
                else
                {
                    LasBitas = resultado.OrderByDescending(x => x.Fecha).ToList();

                    var nombres = resultado!.Where(x => !string.IsNullOrEmpty(x.UserId))
                        .GroupBy(x => x.UserId).Select(x => x.Key).ToList();
                    foreach (var n in nombres)
                    {
                        if (string.IsNullOrEmpty(n) || n.Length < 15) continue;
                        if (!DicData.ContainsKey($"UserId_FullName_{n}"))
                        {
                            var nom = LosUsers.FirstOrDefault(x => x.UserId == n);
                            if (nom != null)
                            {
                                var elnombre = $"{nom.Nombre} {nom.Paterno} ";
                                elnombre += nom.Materno ?? "";
                                DicData.TryAdd($"UserId_FullName_{n}", elnombre);
                            }
                        }
                    }
                }
                Leyendo = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Leyendo = false;
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar leer los registros de la bitacora, {TBita}, {ex}",
                        Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
                LosUsers = new();
            }
        }

        protected async Task LeerLogs(FiltroLog datos)
        {
            try
            {
                Leyendo = true;
                IEnumerable<Z192_Logs> resultado = new List<Z192_Logs>();

                if (datos.Datos)
                {
                    if (datos.Rango)
                    {
                        datos.FFin = datos.FIni > datos.FFin ?
                            datos.FIni.AddDays(1) : datos.FFin;
                    }
                    else
                    {
                        datos.FFin = datos.FIni.AddDays(1);
                    }

                    resultado = await LogRepo.Get(x => x.EmpresaId == ElUser.OrgId &&
                        x.Fecha >= datos.FIni &&
                        x.Fecha <= datos.FFin &&
                        x.UserId == (string.IsNullOrEmpty(datos.UserId) ?
                        x.UserId : datos.UserId) &&
                        x.OrgId == (string.IsNullOrEmpty(datos.OrgId) ?
                        x.OrgId : datos.OrgId) &&
                        x.Corporativo == (string.IsNullOrEmpty(Corporativo) ?
                        x.Corporativo : datos.Corporativo) &&
                        x.Desc.Contains(string.IsNullOrEmpty(datos.Desc) ?
                        x.Desc : datos.Desc));
                }
                else
                {
                    resultado = await LogRepo.Get(x => x.EmpresaId == ElUser.OrgId);
                }

                if (resultado == null || !resultado.Any())
                {
                    LosLogs.Clear();
                }
                else
                {
                    LosLogs = resultado.OrderByDescending(x => x.Fecha).ToList();

                    var nombres = resultado!.Where(x => !string.IsNullOrEmpty(x.UserId))
                        .GroupBy(x => x.UserId).Select(x => x.Key).ToList();
                    foreach (var n in nombres)
                    {
                        if (string.IsNullOrEmpty(n) || n.Length < 15) continue;
                        if (!DicData.ContainsKey($"LogUserId_FullName_{n}"))
                        {
                            var nom = LosUsers.FirstOrDefault(x => x.UserId == n);
                            if (nom != null)
                            {
                                var elnombre = $"{nom.Nombre} {nom.Paterno} ";
                                elnombre += nom.Materno ?? "";
                                DicData.TryAdd($"LogUserId_FullName_{n}", elnombre);
                            }
                        }
                    }
                }
                Leyendo = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar leer los registros de los Logs de errores, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
                LosLogs = new();
            }
        }

        public class Filtro : Z190_Bitacora
        {
            public DateTime FIni { get; set; } = DateTime.Today;
            public DateTime FFin { get; set; } = DateTime.Today.AddDays(1);
            public bool Rango { get; set; }
            public bool Datos { get; set; } = false;
        }
        public class FiltroLog : Z192_Logs
        {
            public DateTime FIni { get; set; } = DateTime.Today;
            public DateTime FFin { get; set; } = DateTime.Today.AddDays(1);
            
            public bool Rango { get; set; }
            public bool Datos { get; set; } = false;
        }


        #region Usuario y Bitacora

        [CascadingParameter(Name = "ElUserAll")]
        protected Z110_User ElUser { get; set; } = new();
        
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
                    if (elId == null || elId == "Vacio")
                        NM.NavigateTo("Identity/Account/Login?ReturnUrl=/", true);
                    ElUser = await UserRepo.GetById(elId!);

                    if (ElUser == null)
                    {
                        NM.NavigateTo("Identity/Account/Login?ReturnUrl=/", true);
                    }
                    /*
                    else if (ElUser.Nivel > 4)
                    {
                        NM.NavigateTo("/indexz", true);
                    }
                    */
                    else
                    {
                        Corporativo = ElUser.OrgId;
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
                 $"Error al intentar leer EL USER USUARIO de la bitacora, {TBita}, {ex}",
                 "All", ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }
        }


        #endregion

    }
}

