
using System;
using System.Security.Claims;
using System.Security.Policy;
using DashBoard.Data;
using DashBoard.Modelos;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using Radzen;

namespace DashBoard.Pages.Zuver
{
	public class UserAddBase : ComponentBase
	{
        public const string TBita = "Agregar Usuario";
        [Inject]
        public IAddUser AddUserRepo { get; set; } = default!;
        [Inject]
        public Repo<Z110_User, ApplicationDbContext> UserRepo { get; set; } = default!;
        [Inject]
        public Repo<ZConfig, ApplicationDbContext> ConfRepo { get; set; } = default!;
        [Inject]
        public IEnviarMail SendMail { get; set; } = default!;

        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        public List<Z100_Org> LasOrgsTmp { get; set; } = new List<Z100_Org>();
        public List<Z100_Org> LasCorpTmp { get; set; } = new List<Z100_Org>();

        [Parameter]
        public List<Z110_User> LosUsers { get; set; } = new List<Z110_User>();

        [Parameter]
        public List<KeyValuePair<string, string>> TipoOrgs { get; set; } =
            new List<KeyValuePair<string, string>>();

        [Parameter]
        public List<KeyValuePair<int, string>> NivelesEdit { get; set; } =
            new List<KeyValuePair<int, string>>();

        public AddUser NuevoUser { get; set; } = new();

        public bool Editando { get; set; } = false;

        [Parameter]
        public EventCallback ReadUsersAll  { get; set; }
        [Parameter]
        public EventCallback ReadOrgsAll { get; set; }

        
        public bool BotonGo { get; set; } = false;
        protected bool MailExiste { get; set; } = false;
        protected string WrongPass { get; set; } = "";
        protected string WrongConfirm { get; set; } = "";
        protected bool Primera { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                if (!LasOrgs.Any() || !LosUsers.Any())
                    await ReadOrgsAndUsers();
            }
            await Leer();
        }

        public async Task Leer()
        {
            await ReadNiveles();
            await ReadOrgs();
            await ReadCorps();
            ReadTipos();

        }

        public void ReadBotonGo()
        {
            BotonGo = true;
            BotonGo = BotonGo && !MailExiste && WrongPass == "" && WrongConfirm == "";
            BotonGo = BotonGo && NuevoUser.Mail.Length > 5 && NuevoUser.Nombre.Length > 1 && NuevoUser.Paterno.Length > 1;
            BotonGo = BotonGo && NuevoUser.OrgId.Length > 15 && NuevoUser.Nivel > 0;
            if (BotonGo)
                StateHasChanged();
        }

        public async Task ReadNiveles()
        {
            try
            {
                if (NivelesEdit.Any())
                    NivelesEdit = NivelesEdit.Where(x => x.Key < ElUser.Nivel).Select(x => x).ToList();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al leer los niveles para los usuarios, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        public async Task ReadOrgs()
        {
            try
            {
                if (LasOrgs.Any())
                
                    LasOrgsTmp = LasOrgs.Where(x => x.Corporativo ==
                                    (ElUser.Nivel < 5 ? ElUser.OrgId : x.Corporativo))
                                    .Select(x => x).ToList();          
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al leer las organizaciones, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        public async Task ReadCorps()
        {
            try
            {
                LasCorpTmp = LasOrgs.Any() ?
                    LasOrgs.Where(x => x.Tipo == "Administracion" && x.Estado == 1 && x.Status == true).ToList() :
                    new List<Z100_Org>();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al leer las organizaciones de corporativo, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        public async Task ReadOrgsAndUsers()
        {
            try
            {
                await ReadOrgsAll.InvokeAsync();
                await ReadUsersAll.InvokeAsync();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al leer las organizaciones y Usuarios en un solo comando, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        
        public async Task<ApiRespuesta<ZConfig>> EmpActAddUser(ZConfig datos)
        {
            ApiRespuesta<ZConfig> resultado = new() { Exito = false };
            try
            {
                if (datos == null)
                {
                    resultado.MsnError.Add($"No se agrego Empresa activa ");
                }
                else
                {
                    var resp = await ConfRepo.Insert(datos);
                    if (resp != null)
                    {
                        resultado.Exito = true;
                        resultado.Data = resp;
                    }
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                            $"Error al intentar agregar una empresa activa al usuario creado, {TBita}, {ex}",
                            Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
            return resultado;
        }

        public async Task UpdateUsers() => await ReadUsersAll.InvokeAsync();

        public async void ReadTipos()
        {
            try
            {
                string[] tipos = Constantes.OrgTipo.Split(",");
                foreach(var t in tipos)
                {
                    if (t == "Administracion" && ElUser.Nivel < 5)
                        continue;
                    else
                        TipoOrgs.Add(new KeyValuePair<string, string>(t, t));
                }
                
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al leer los tipos de organizaciones, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        public async void CheckMail(string email)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                    MailExiste = LosUsers.Any(x => x.OldEmail == email);
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al checar el Email nuevo, YA EXISTE, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
            
        }

        public async void CheckPass(string Password)
        {
            try
            {
                WrongPass = "";
                
                var options = new IdentityOptions
                {
                    Password = new PasswordOptions
                    {
                        RequireDigit = true,
                        RequireNonAlphanumeric = false,
                        //RequiredLength = 6,
                        RequiredUniqueChars = 4,
                        RequireLowercase = true,
                        RequireUppercase = true
                    }
                };
                
                if (options.Password.RequireDigit && !Password.Any(char.IsDigit))
                {
                    // La contraseña no contiene dígitos.

                    WrongPass += "Es necesario un digito en el password! ";
                }

                if (options.Password.RequiredUniqueChars > 0 &&
                    Password.Distinct().Count() < options.Password.RequiredUniqueChars)
                {
                    // La contraseña no tiene suficientes caracteres únicos.
                    WrongPass += "Es necesario que no se repitan los caracteres! ";
                }
                if (options.Password.RequireLowercase && !Password.Any(char.IsLower))
                {
                    WrongPass += "Es necesario una minuscula! ";
                }
                if (options.Password.RequireUppercase && !Password.Any(char.IsUpper))
                {
                    WrongPass += "Es necesario una mayuscula! ";
                }
                CheckConfirm();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al checar el password nuevo, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        public async void CheckConfirm()
        {
            try
            {
                WrongConfirm = "";
                if (!string.IsNullOrEmpty(NuevoUser.Pass) && NuevoUser.Pass != NuevoUser.ConfirmPass)
                    WrongConfirm = "El Password y su confirmacion no son iguales!";
                ReadBotonGo();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al checar la confirmacion del password, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        public async Task<ApiRespuesta<Z110_User>>InsertUser(Z110_User nUser)
        {
            ApiRespuesta<Z110_User> resultado = new();
            try
            {
                if (nUser != null)
                {
                    Z110_User userInsert = await UserRepo.Insert(nUser);
                    resultado.Exito = true;
                    resultado.Data = userInsert;
                }
                else
                {
                    resultado.Exito = false;
                    resultado.MsnError.Add($"No se pudo insertar el usuario");
                }
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.MsnError.Add(ex.Message);
            }
            return resultado;
        }

        public async Task<ApiRespuesta<AddUser>> Servicio(ServiciosTipos tipo, AddUser newD)
        {
            ApiRespuesta<AddUser> resp = new();

            try
            {
                if (newD != null)
                {
                    if (tipo == ServiciosTipos.Crear)
                    {
                        ApiRespuesta<AddUser> userCreate = await AddUserRepo.CrearNewAcceso(newD);
                        if (userCreate.Exito)
                        {
                            resp.Exito = true;
                            resp.Data = userCreate.Data;

                            Z110_User nUser = new()
                            {
                                UserId = userCreate.Data.UserId,
                                Nombre = newD.Nombre,
                                Paterno = newD.Paterno,
                                Materno = string.IsNullOrEmpty(newD.Materno) ? "" : newD.Materno,
                                Nivel = newD.Nivel,
                                OrgId = newD.OrgId,
                                OldEmail = newD.Mail
                            };
                            ApiRespuesta<Z110_User> uResp =  await InsertUser(nUser);
                            if (!uResp.Exito)
                            {
                                resp.Exito = false;
                                foreach (var e in uResp.MsnError)
                                {
                                    resp.MsnError.Add($"{e}");
                                }
                                
                            }
                        }
                        else
                        {
                            resp.MsnError.Add("No puedo agregarse un nuevo USER");
                            foreach (var e in userCreate.MsnError)
                            {
                                resp.MsnError.Add($"{e}");
                            }
                           
                        }
                        return resp;
                    }
                }
                resp.Exito = false;
                resp.MsnError.Add("Sin valor para crear un nuevo acceso");
                
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar AGREGAR un nuevo USER, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
                resp.Exito = false;
                
            }
            return resp;
        }

        public class NuevoUsuario : Z110_User
        {
            public string url { get; set; } = "";
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
                    $"Error al intentar escribir LOGbitacora, {TBita},{ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }

        }
        #endregion

        
    }
}
