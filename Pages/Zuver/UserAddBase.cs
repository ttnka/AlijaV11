using System;
using System.Security.Claims;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Radzen;

namespace DashBoard.Pages.Zuver
{
	public class UserAddBase : ComponentBase
	{
        public const string TBita = "Agregar Usuario";
        [Inject]
        public IAddUser AddUserRepo { get; set; } = default!;

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

        [CascadingParameter(Name = "CorporativoAll")]
        public string Corporativo { get; set; } = "All";

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
                await LogRepo.Insert(LogT);
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
                await LogRepo.Insert(LogT);
            }
        }

        public async Task ReadCorps()
        {
            try
            {
                /*
                if (LasOrgs.Any())
                    LasOrgsTmp = ElUser.Nivel < 5 ? 
                            LasOrgs.Where(x => x.Corporativo == ElUser.Corporativo).GroupBy(x=>x.Corporativo)
                            .Select(x => x.First()).ToList() :

                            LasOrgs.Where(x => x.Estado == 1).GroupBy(x=>x.Corporativo).Select(x => x.First()).ToList();
                */
                LasOrgsTmp = LasOrgs;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al leer las organizaciones de corporativo, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
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
                await LogRepo.Insert(LogT);
            }
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
                await LogRepo.Insert(LogT);
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
                await LogRepo.Insert(LogT);
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
                /*
                if (password.Length < options.Password.RequiredLength)
                {
                    // La contraseña es demasiado corta.
                    WorngPass[0] = "1";
                    WorngPass[1] = "Password minimo 6 caracteres";
                }
                */
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
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al checar el password nuevo, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
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
                await LogRepo.Insert(LogT);
            }
        }

        public async Task<ApiRespuesta<AddUser>> Servicio(string tipo, AddUser newD)
        {
            ApiRespuesta<AddUser> resp = new()
            {
                Exito = false,
                Data = newD
            };

            try
            {
                if (newD != null)
                {
                    if (tipo == "Insert")
                    {
                        ApiRespuesta<AddUser> userInsert = await AddUserRepo.InsertNewUser(newD);
                        if (userInsert != null)
                        {
                            return userInsert;
                        }
                        else
                        {
                            resp.MsnError.Add("No puedo agregarse un nuevo USER");
                            return resp;
                        }
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar AGREGAR un nuevo USER, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
                resp.Exito = false;
                return resp;
            }
        }

        #region Usuario y Bitacora
        [Inject]
        public Repo<Z110_User, ApplicationDbContext> UserRepo { get; set; } = default!;

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
                await LogRepo.Insert(LogT);
            }
        }


        #endregion

    }
}

