using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Zuver
{
	public class OrgListBase : ComponentBase
	{
        public const string TBita = "Organizaciones";
        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgRepo { get; set; } = default!;

        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        public List<Z100_Org> LasOrgsCorp { get; set; } = new List<Z100_Org>();
        [Parameter]
        public List<Z110_User> LosUsuarios { get; set; } = new List<Z110_User>();

        
        [Parameter]
        public List<KeyValuePair<int, string>> NivelesEdit { get; set; } =
            new List<KeyValuePair<int, string>>();

        [Parameter]
        public List<KeyValuePair<string, string>> TipoOrgs { get; set; } =
            new List<KeyValuePair<string, string>>();
        
        [Parameter]
        public EventCallback LeerOrgAll { get; set; }
        [Parameter]
        public EventCallback LeerUsersAll { get; set; }

        public RadzenDataGrid<Z100_Org>? OrgGrid { get; set; } =
            new RadzenDataGrid<Z100_Org>();

        [CascadingParameter(Name = "CorporativoAll")]
        public string Corporativo { get; set; } = "All";

        
        protected bool Editando = false;
        protected bool ShowAdd { get; set; } = false;
        protected string txtNewOrg = "Nueva Empresa";
        protected bool Primera = true;
        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                if (LasOrgs.Count == 0)
                    await GoLeerOrgs();
            }
            await Leer();

            Primera = false;
            var bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                $"El usuario consulto listado de {TBita}", Corporativo, false);
            await BitacoraAll(bitaTemp);
        }

        public async Task Leer()
        {
            await PoblarLasOrgsCorp();
        }


        protected async Task PoblarLasOrgsCorp()
        {
            try
            {
                if (LasOrgs.Any())
                {
                    LasOrgsCorp = ElUser.Nivel < 5 ?
                        LasOrgs.Where(x => x.Corporativo == ElUser.Corporativo)
                        .Select(x => x).ToList() :
                        LasOrgs;
                }
            }
            catch (Exception ex)
            {
                var LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar generar el listado de corporativos, {TBita}, {ex}",
                    Corporativo, true);
                await LogRepo.Insert(LogT);
            }
        }
        protected async Task LeerOrgsAndUsers()
        {
            await LeerOrgAll.InvokeAsync();
            await LeerUsersAll.InvokeAsync();
        }
        protected async Task GoLeerOrgs()
        {
            await LeerOrgAll.InvokeAsync();
            await OrgGrid!.Reload();
        }

        protected async Task GoLeerUsers()
        {
            await LeerUsersAll.InvokeAsync();
        }

        public async Task<ApiRespuesta<Z100_Org>> Servicio(string tipo, Z100_Org org)
        {
            ApiRespuesta<Z100_Org> resp = new()
            {
                Exito = false,
                Data = org
            };

            try
            {
                if (org != null )
                {
                    if (tipo == "Insert")
                    {
                        org.OrgId = Guid.NewGuid().ToString();
                        var orgInsert = await OrgRepo.Insert(org);
                        if (orgInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = orgInsert;
                        }
                        else
                        {
                            resp.MsnError.Add("No se Inserto el registro ");
                        }
                        return resp;
                    }
                    else if (tipo == "Update")
                    {
                        if (LasOrgs.Exists(x=>x.Rfc.ToUpper() == org.Rfc.ToUpper()
                        && x.OrgId != org.OrgId))
                        {
                            resp.MsnError.Add("El RFC ya esta REGISTRADO!");
                            return resp;
                        }
                        var orgUpdate = await OrgRepo.Update(org);
                        if (orgUpdate != null)
                        {
                            resp.Exito = true;
                        }
                        else
                        {
                            resp.MsnError.Add("No se Actualizo el registro ");
                        }
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                resp.MsnError.Add(ex.Message);
                var LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar {tipo} los registros de {TBita} {ex}",
                        Corporativo, true);
                await LogRepo.Insert(LogT);
                return resp;
            }

        }

        #region Usuario y Bitacora

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
            switch (tipo.ToLower())
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
        public async Task BitacoraAll(Z190_Bitacora bita)
        {
            try
            {
                if (bita.Fecha.Subtract(LastBita.Fecha).TotalSeconds > 15 ||
                    LastBita.Desc != bita.Desc || LastBita.Sistema != bita.Sistema ||
                    LastBita.UserId != bita.UserId || LastBita.OrgId != bita.OrgId)
                {
                    LastBita = bita;
                    await BitaRepo.Insert(bita);
                }
            }
            catch (Exception ex)
            {
                var LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar escribir BITACORA, {TBita},{ex}",
                    Corporativo, true);
                await LogRepo.Insert(LogT);
            }
            
        }
        #endregion

    }
}

