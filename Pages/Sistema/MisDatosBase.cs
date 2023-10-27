using System;
using System.Security.Claims;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Radzen;
using static DashBoard.Pages.Zuver.OrgAddBase;

namespace DashBoard.Pages.Sistema
{
	public class MisDatosBase : ComponentBase
	{
        public const string TBita = "Mis Datos";
        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgsRepo { get; set; } = default!;
        [Inject]
        public Repo<Z110_User, ApplicationDbContext> UserRepo { get; set; } = default!;
        public Z110_User OldDataUser { get; set; } = new();

        [Parameter]
        public List<KeyValuePair<int, string>> Niveles { get; set; } =
            new List<KeyValuePair<int, string>>();
        [Parameter]
        public List<Z100_Org> LasOrg { get; set; } = new List<Z100_Org>();
        public List<Z100_Org> LasOrgCorp { get; set; } = new List<Z100_Org>();
        public List<Z100_Org> LosAlijadores { get; set; } = new List<Z100_Org>();

        public bool Editando { get; set; } = false;

        [CascadingParameter(Name = "CorporativoAll")]
        public string Corporativo { get; set; } = "All";

        protected bool PrimeraVez { get; set; } = true;
        protected override async Task OnInitializedAsync()
        {
            if (ElUser == null || ElUser.UserId.Length < 15)
                await LeerElUser();
            else
                await Leer();
            
        }

        protected async Task Leer()
        {
            try
            {
                await UserToOldData();
                await ReadNiveles();
                await ReadOrgCorp(); // Se lee los ALIJADORES que son empresas ADMINISTADORAS
                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                        $"Consulto {TBita}", Corporativo, ElUser.OrgId);
                await BitaRepo.Insert(bitaTemp);
                
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar leer los datos USER, {TBita},{ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }


        protected async Task ReadNiveles()
        {
            try
            {
                
                if (Niveles.Any())
                {
                    Niveles = Niveles.Where(x => x.Key <= ElUser.Nivel).Select(x => x).ToList();
                }
                
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar leer los datos de NIVELES, {TBita},{ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task ReadOrgCorp()
        {
            try
            {
                if(LasOrg.Any())
                {
                    LasOrgCorp = LasOrg.GroupBy(x => x.Corporativo)
                        .Select(x => x.First()).ToList();
                    LosAlijadores = LasOrg.Where(x => x.Tipo == "Administracion").ToList();
                }

            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar leer los datos de las corporaciones, {TBita},{ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task UserToOldData()
        {
            try
            {
                OldDataUser.UserId = ElUser.UserId.ToString();
                OldDataUser.OrgId = ElUser.OrgId.ToString();
                OldDataUser.Nombre = ElUser.Nombre.ToString();
                OldDataUser.Paterno = ElUser.Paterno.ToString();
                OldDataUser.Materno = ElUser.Materno != null ? ElUser.Materno.ToString() : "";
                OldDataUser.Nivel = ElUser.Nivel;
                OldDataUser.OldEmail = ElUser.OldEmail.ToString();
                OldDataUser.Estado = ElUser.Estado;
                
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar leer los datos del USER, {TBita},{ex}",
                   Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
            
        }

        protected async Task<string> UserTxt(bool nuevo, Z110_User data)
        {
            string resp = "";
            try
            {
                string nivelT = !Niveles.Any() || data.Nivel > Niveles.Count +1 || !Niveles.Exists(x => x.Key == data.Nivel) ?
                    "Sin Nivel" : Niveles.FirstOrDefault(x => x.Key == data.Nivel).Value;
                string orgIdT =  !LasOrg.Any() || !LasOrg.Exists(x=>x.OrgId == data.OrgId) ?
                    "Sin Organizacion" : LasOrg.FirstOrDefault(x => x.OrgId == data.OrgId)!.ComercialRfc;
                string statusT = data.Status ? "Activo" : "Suspendido";
                if (nuevo)
                {
                    resp = $"El NUEVO registro contiene: Nombre: {data.Completo} Email: {data.OldEmail}, ";
                    resp += $"nivel: {nivelT}, organizacion: {orgIdT} ";
                }
                else
                {
                    resp = $"El registro Original era: Nombre: {data.Completo} Email: {data.OldEmail}, ";
                    resp += $"nivel: {nivelT}, organizacion: {orgIdT} ";
                }
                resp += $"Estado: {data.Estado}, Estatus: {statusT}";
                
                return resp;
            }
            catch (Exception ex)
            {
                resp = "Error";
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar llevar los datos del USER a texto 'UserTxt', {TBita},{ex}",
                   Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                return resp;
            }
        }

        public async Task<ApiRespuesta<Z110_User>> Servicio(string tipo, Z110_User newD)
        {
            ApiRespuesta<Z110_User> resp = new()
            {
                Exito = false,
                Data = newD
            };

            try
            {
                if (newD != null)
                {
                    if (tipo == "Update")
                    {
                        Z110_User userUpDate = await UserRepo.Update(newD);
                        if (userUpDate != null)
                        {
                            resp.Exito = true;
                            resp.Data = userUpDate;
                            return resp;
                        }
                        else
                        {
                            resp.MsnError.Add("No puedo actualizar los datos del USER");
                            return resp;
                        }
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar actualizar los datos del USER, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                resp.Exito = false;
                return resp;
            }
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
                await LogAll(LogT);
            }
        }


        #endregion


    }
}

