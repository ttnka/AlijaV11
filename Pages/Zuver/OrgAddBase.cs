using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Zuver
{
	public class OrgAddBase : ComponentBase
	{
        public const string TBita = "Agregar Organizaciones";

        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgRepo { get; set; } = default!;

        [Inject]
        public IAddUser AddUserRepo { get; set; } = default!;

        [Parameter]
        public Dictionary<string, string> DicData { get; set; } =
            new Dictionary<string, string>();
        [Parameter]
        public List<KeyValuePair<string, string>> TipoOrgs { get; set; } =
            new List<KeyValuePair<string, string>>();

        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        public List<Z100_Org> LasOrgsCorp { get; set; } = new List<Z100_Org>();

        [Parameter]
        public List<Z110_User> LosUsres { get; set; } = new List<Z110_User>();

        
        [Parameter]
        public EventCallback LeerOrgAndUserAll { get; set; }
        
        [Parameter]
        public EventCallback OcultarBoton { get; set; } = default!;
        [Parameter]
        public EventCallback LeerAdmin { get; set; }
        [CascadingParameter(Name = "CorporativoAll")]
        public string Corporativo { get; set; } = "Buscar";
        [Parameter]
        public bool EsAdministrador { get; set; } = false;

        public LaOrgNew OrgNew { get; set; } = new();
        public bool BotonNuevo = false;
        public string EMsn { get; set; } = "";
        
        public bool ShowGpo { get; set; } = false;
        public bool Editando = false;
        
        protected bool Primera { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            Leer();
            var bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId,
                 $"Entro a generar una nueva organizacion, {TBita}", Corporativo, false);
            await BitacoraAll(bitaTemp);
        }

        public void Leer()
        {
            PoblarLasOrgsCorp();
            PoblarLosTipos();
        }

        public void PoblarLosTipos()
        {
            if (TipoOrgs.Any())
            {
                if (ElUser.Nivel < 6)
                    TipoOrgs.RemoveAll(x => x.Key == "Administracion");
            }
        }

        public void PoblarLasOrgsCorp()
        {
            if (LasOrgs.Any())
            {
                LasOrgsCorp = ElUser.Nivel < 5 ?
                    LasOrgs.Where(x => x.Corporativo == ElUser.Corporativo)
                    .Select(x => x).ToList() :
                    LasOrgs.Where(x=>x.Estado == 1).Select(x=>x).ToList();       
            }
        }
        public void ChecarFormato()
        {
            EMsn = "";
            // Validar si ya existe el RFC
            EMsn += LasOrgs.Exists(x => x.Rfc.ToUpper() == OrgNew.Rfc.ToUpper()) ?
                "El RFC ya esta registrado! -" : "";
            EMsn += LasOrgs.Exists(x => x.Comercial.ToUpper() == OrgNew.Comercial.ToUpper()) ?
                "El Nombre comercial ya existe! -" : "";
            EMsn += LosUsres.Exists(x => x.OldEmail!.ToUpper() == OrgNew.UserOldEmail.ToUpper()) ?
                "El EMAIL ya existe! -" : "";
        }

        public async Task<ApiRespuesta<LaOrgNew>> Servicio(string tipo, LaOrgNew org)
        {
            ApiRespuesta<LaOrgNew> resp = new()
            {
                Exito = false,
                Data = org
            };

            try
            {
                if(org != null )
                {
                    if (tipo == "Insert")
                    {
                        if (LasOrgs.Exists(x => x.Rfc.ToUpper() == org.Rfc.ToUpper()))
                        {
                            resp.MsnError.Add("El RFC YA EXISTE!");
                            return resp;
                        }
                        if (EMsn != "")
                        {
                            resp.MsnError.Add(EMsn);
                            return resp;
                        }
                        Z100_Org orgAdd = new();
                        orgAdd.OrgId = org.OrgId;
                        orgAdd.Rfc = org.Rfc;
                        orgAdd.Comercial = org.Comercial;
                        orgAdd.RazonSocial = org.RazonSocial;
                        orgAdd.Moral = org.Moral;
                        orgAdd.NumCliente = org.NumCliente;
                        orgAdd.Tipo = org.Tipo;
                        orgAdd.Corporativo = org.Corporativo;
                        orgAdd.Estado = org.Estado;

                        var orgInsert = await OrgRepo.Insert(orgAdd);
                        if (orgInsert == null)
                        {
                            resp.MsnError.Add("No puedo agregarse la empresa");
                            return resp;
                        }
                        else
                        {
                            AddUser eAddUsuario = new();
                            eAddUsuario.Mail = org.UserOldEmail;
                            eAddUsuario.Pass = $"{org.Rfc}_Mm1";
                            eAddUsuario.OrgId = org.OrgId;
                            eAddUsuario.Nombre = org.UserNombre;
                            eAddUsuario.Paterno = org.UserPaterno;
                            eAddUsuario.Materno = org.UserMaterno;
                            eAddUsuario.Corporativo = org.Corporativo;
                            if (EsAdministrador)
                            {
                                if (org.Tipo == "Cliente")
                                {
                                    eAddUsuario.Nivel = 4;
                                }
                                else if (org.Tipo == "Proveedor")
                                {
                                    eAddUsuario.Nivel = 2;
                                }
                                else
                                {
                                    eAddUsuario.Nivel = 6;
                                }
                            }
                            else
                            {
                                eAddUsuario.Nivel = org.UserNivel;
                            }

                            var UserInsert = await AddUserRepo.InsertNewUser(eAddUsuario);
                            if (!UserInsert.Exito)
                            {
                                resp.MsnError.Add("No se agrego un usuario de la nueva organizacion");
                                return resp;    
                            }
                        }
                    }
                }
                resp.Exito = true;
                return resp;
            }
            catch (Exception ex)
            {
                var LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar agregar una organizacion y un usuario, {TBita}, {ex}",
                    Corporativo, true);
                await LogRepo.Insert(LogT);
                resp.Exito = false;
                return resp;
            }
        }

        public class LaOrgNew : Z100_Org

        {
            [StringLength(50)]
            public string UserId { get; set; } = "";
            [StringLength(50)]
            public string UserNombre { get; set; } = "";
            [StringLength(50)]
            public string UserPaterno { get; set; } = "";
            [StringLength(50)]
            public string? UserMaterno { get; set; }
            public int UserNivel { get; set; } = 1;

            public string UserOldEmail { get; set; } = "";

            public int UserEstado { get; set; } = 2;
            public bool USerStatus { get; set; } = true;
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

        #region Pertenece solo a esta Pagina
        public void Cancelar()
        {
            //OrgNew = new();
            OrgNew.OrgId = "";
            OrgNew.Rfc = "";
            OrgNew.Comercial = "";
            OrgNew.RazonSocial = "";
            OrgNew.NumCliente = "";
            OrgNew.Tipo = "";
            OrgNew.Corporativo = "All";
            OrgNew.UserId = "";
            OrgNew.UserNombre = "";
            OrgNew.UserPaterno = "";
            OrgNew.UserMaterno = "";
            OrgNew.UserOldEmail = "";
            OrgNew.UserNivel = 1;
            ChecarFormato();
            StateHasChanged();
        }

        
        #endregion


    }
}

