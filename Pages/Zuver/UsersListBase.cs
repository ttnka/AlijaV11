using System;
using System.Text.RegularExpressions;
using DashBoard.Data;
//using DashBoard.Migrations;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Zuver
{
	public class UsersListBase : ComponentBase 
	{
        public const string TBita = "Lista Usuarios";

        [Inject]
        public Repo<Z110_User, ApplicationDbContext> UserRepo { get; set; } = default!;
        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new();
        public List<Z100_Org> LasEmp { get; set; } = new();
        [Parameter]
        public List<Z110_User> LosUsuarios { get; set; } = new List<Z110_User>();
        public List<Z110_User> LosUsersTmp { get; set; } = new();
        
        [Parameter]
        public List<KeyValuePair<string, string>> TipoOrgs { get; set; } =
            new List<KeyValuePair<string, string>>();
        [Parameter]
        public List<KeyValuePair<int, string>> NivelesEdit { get; set; } =
            new List<KeyValuePair<int, string>>();
        public List<KeyValuePair<int, string>> NivelesTemp { get; set; } =
            new List<KeyValuePair<int, string>>();
        [Parameter]
        public string LaOrg { get; set; } = "";
        
        [Parameter]
        public EventCallback LeerOrgAll { get; set; }
        [Parameter]
        public EventCallback LeerUsersAll { get; set; }
        public RadzenDataGrid<Z110_User>? UserGrid { get; set; } =
            new RadzenDataGrid<Z110_User>();

        public bool ShowAdd = false;
        public string txtShowAdd = "Nuevo usuario";
        public bool Editando = false;
        public bool Primera = true;

        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (!LosUsuarios.Any())
                    await UsersAllRead();
            }
            await Leer();
        }
        public async Task Leer()
        {
            try
            {
                FiltraUsers();
                FiltraNiveles();
                LasEmp = ElUser.Nivel > 5 ? LasOrgs : LasOrgs.Where(x => x.OrgId == ElUser.OrgId).ToList();

                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                    $"Consulto listado, {TBita}", Corporativo, ElUser.OrgId);
                await BitacoraAll(bitaTemp);
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar leer los registros de {TBita} {ex}",
                        Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
            
        }

        public void FiltraUsers()
        {
            if (LosUsuarios.Any())
            {
                /*
                LosUsersTmp = ElUser.Nivel < 5 ?
                    LosUsuarios.Where(x => x.Corporativo == Corporativo)
                    .Select(x => x).ToList() :
                    LosUsuarios;
                */
                LosUsersTmp = LosUsuarios;
            }
        }

        public void FiltraNiveles()
        {
            if (NivelesEdit.Any())
            {
                NivelesTemp = NivelesEdit.Where(x => x.Key < ElUser.Nivel).Select(x => x).ToList();
            }
        }

        public async Task UsersAllRead()
        {
            await LeerUsersAll.InvokeAsync();
        }

        public async Task<ApiRespuesta<Z110_User>> Servicio(ServiciosTipos tipo, Z110_User user)
        {
            ApiRespuesta<Z110_User> resp = new()
            {
                Exito = false,
                Data = user
            };

            try
            {
                if (user != null)
                {
                    if (tipo == ServiciosTipos.Insert)
                    {
                        resp.MsnError.Add("El modulo de listado de usuarios no agrega usuarios nuevos");
                        return resp;
                    }
                    else if (tipo == ServiciosTipos.Update)
                    {
                        Z110_User userUpdate = await UserRepo.Update(user);
                        if (userUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = userUpdate;
                            return resp;
                        }
                        else
                        {
                            string txt = $"No se Actualizo el registro {user.Nombre} {user.Paterno}";
                            txt += $"organizacion:{user.OrgId}";
                            resp.MsnError.Add(txt);
                            resp.Exito = false;
                            resp.Data = user;
                            return resp;
                        }
                    }
                }
                resp.MsnError.Add("Ninguna OPERACION se realizo al intentar actualizar una organizacion!");
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

