using System;
using System.Security.Policy;
using System.Text;
using DashBoard.Data;
using MailKit.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;

namespace DashBoard.Modelos
{
	public class RAddUser : IAddUser
	{
        private readonly ApplicationDbContext _appDbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly NavigationManager _navigationManager;

        public RAddUser(ApplicationDbContext appDbContext,
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender,
            NavigationManager navigationManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _emailSender = emailSender;
            _navigationManager = navigationManager;
        }
        public MyFunc MyFunc { get; set; } = new();

        public async Task<ApiRespuesta<AddUser>> FirmaIn(AddUser addUsuario)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiRespuesta<AddUser>> InsertNewUser(AddUser UsuarioNuevo)
        {
            ApiRespuesta<AddUser> apiRespuesta = new()
                {
                    Exito = false,
                    MsnError = new List<string>(),
                    Data = UsuarioNuevo
                };
            try
            {
                
                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, UsuarioNuevo.Mail, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, UsuarioNuevo.Mail, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, UsuarioNuevo.Pass);
                if (result.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);

                    Z110_User userUpDate = new();
                    userUpDate.UserId = userId;
                    userUpDate.Nombre = UsuarioNuevo.Nombre!;
                    userUpDate.Paterno = UsuarioNuevo.Paterno!;
                    userUpDate.Materno = UsuarioNuevo.Materno == null ? "" : UsuarioNuevo.Materno;
                    userUpDate.OldEmail = user.Email;
                    userUpDate.OrgId = UsuarioNuevo.OrgId!;
                    userUpDate.Nivel = UsuarioNuevo.Nivel;
                    userUpDate.Corporativo = UsuarioNuevo.Corporativo;
                    userUpDate.Estado = UsuarioNuevo.Sistema ? 1 : 2;

                    // Agrega el Usuario nuevo a tabla de Users con los datos ASP USER NET
                    try
                    {
                        await _appDbContext.AddAsync(userUpDate);
                        var sav1 = await _appDbContext.SaveChangesAsync();

                        apiRespuesta.Data.UsuarioId = userId;
                        apiRespuesta.Exito = true;
                    }
                    catch (Exception ex)
                    {
                        var eTxt = $"Se genero un error al intentar crear un nuevo usuario {ex}";
                        var userT = userId ?? "Sin Usuario";
                        var orgT = UsuarioNuevo.OrgId ?? "Sin Organizacion";
                        var corpT = UsuarioNuevo.Corporativo ?? "Sin Corporativo";

                        await WriteBitacora(userT, orgT, eTxt, corpT, false);
                        apiRespuesta.Exito = false;
                        apiRespuesta.MsnError.Add("Hubo un error al intenetar actualizar USUARIOS con el user nuevo");
                        return apiRespuesta;
                    }

                    // Escribe que agrego un usuario nuevo

                    try
                    {
                        var txt = $"Se creo nuevo acceso {UsuarioNuevo.Mail} y password, ";
                        txt += $"{UsuarioNuevo.Nombre} {UsuarioNuevo.Paterno} {UsuarioNuevo.OrgName}";
                        txt += UsuarioNuevo.OrgName == "" ? "El sistema" : UsuarioNuevo.OrgName;
                        await WriteBitacora(userId, UsuarioNuevo.OrgId, txt,
                                            UsuarioNuevo.Corporativo, false);
                    }
                    catch (Exception ex)
                    {
                        var eTxt = $"Se genero un error al intentar escribir en bitacora un nuevo usuario {ex}";
                        var userT = userId ?? "Sin Usuario";
                        var orgT = UsuarioNuevo.OrgId ?? "Sin Organizacion";
                        var corpT = UsuarioNuevo.Corporativo ?? "Sin Corporativo";
                        await WriteBitacora(userT, orgT, eTxt, corpT, true);
                        apiRespuesta.Exito = false;
                        apiRespuesta.MsnError.Add(eTxt);
                        return apiRespuesta;
                    }

                    // Mail para notificar al usuario que se hizo una cuenta

                    MailCampos mailCampos = new();
                    string elCuerpo = "<label>Hola !</label> <br /><br />";
           
                    if (Constantes.EsNecesarioConfirmarMail)
                    {
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var url = ($"{Constantes.ConfirmarMailTxt}{userId}&code={code}");
                        elCuerpo += $"<label>Te escribimos de {Constantes.ElDominio} para enviarte un correo de confirmacion de cuenta </label><br />";
                        elCuerpo += $"<label>por favor confirma tu Cuenta de correo ingresando al siguiente enlace:</label><br />";
                        elCuerpo += $"<a href={url}>Confirma tu Cuenta</a> <br />";

                    }
                    else
                    {
                        elCuerpo += $"<label>Te escribimos de {Constantes.ElDominio} para enviarte un correo de Bienvenida </label><br />";
                        elCuerpo += $"<label>Nuestro sitio te ayuda con {Constantes.SitioDesc}:</label><br />";
                        elCuerpo += $"<label>Navega en nuestro sitio, Te esperamos ";
                    }

                    mailCampos = mailCampos.PoblarMail(UsuarioNuevo.Mail, "Confirma Tu correo!", elCuerpo,
                        UsuarioNuevo.Nombre, userId, UsuarioNuevo.OrgId, UsuarioNuevo.Corporativo, Constantes.DeNombreMail01,
                        Constantes.DeMail01, Constantes.ServerMail01, Constantes.PortMail01,
                        Constantes.UserNameMail01, Constantes.PasswordMail01);


                    var mails = await EnviarMail(mailCampos);
                    if (!mails.Exito)
                    {
                        foreach (var e in mails.MsnError)
                        {
                            apiRespuesta.MsnError.Add($"Hubo un errro al intenetar enviar MAIL {e}");
                        }
                    }
                }
                return apiRespuesta;
            }
            catch (Exception ex)
            {
                var eTxt = $"Error al intentar generar un nuevo usuario {ex}";
                string userIdTmp = UsuarioNuevo.UsuarioId ?? "Sin_Usuario_Error";
                string orgIdTmp = UsuarioNuevo.OrgId ?? "Sin_Organizacion_Error";
                string corpIdTmp = UsuarioNuevo.Corporativo ?? "Sin_Corporativo_Error";
                await WriteBitacora(userIdTmp, orgIdTmp , eTxt, corpIdTmp, true);
                apiRespuesta.Exito = false;
                apiRespuesta.MsnError.Add(eTxt);
                return apiRespuesta;

            }
        }

        public async Task<ApiRespuesta<MailCampos>> EnviarMail(MailCampos mailCampos)
        {
            ApiRespuesta<MailCampos> apiRespuesta = new()
            {
                Exito = false,
                MsnError = new List<string>(),
                Data = mailCampos
            };
            #region EVALUAR Info de envio y cuentas
            if (mailCampos == null)
            {
                apiRespuesta.Exito = false;
                apiRespuesta.MsnError.Add("No hay datos para enviar mail!");
                apiRespuesta.Data = new MailCampos();

                return apiRespuesta;
            }
            if (string.IsNullOrEmpty(mailCampos.SenderEmail))
                apiRespuesta.MsnError.Add("No hay direccion de envio Sender");
            
            if (string.IsNullOrEmpty(mailCampos.Titulo))
                apiRespuesta.MsnError.Add("No hay titulo del mail!");

            if (string.IsNullOrEmpty(mailCampos.Cuerpo))
                apiRespuesta.MsnError.Add("No hay cuerpo del mail");

            if (apiRespuesta.MsnError.Count > 0)
            {
                return apiRespuesta;
            }

            #endregion

            #region Evaluar si es correo de pruebas
            if (mailCampos.Para.EndsWith(".com1") ||
                mailCampos.Para == Constantes.DeMail01)
            {
                apiRespuesta.Exito = true;
                apiRespuesta.MsnError.Add("Email de prueba exitos!");
                await WriteBitacora(mailCampos.UserId, mailCampos.OrgId,
                    "Se supespendio el envio de mail ya que es un correo de prueba!",
                    mailCampos.Corporativo, true);
                return apiRespuesta;
            }
            #endregion
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(mailCampos.SenderEmail));
                email.To.Add(MailboxAddress.Parse(mailCampos.Para));
                email.Subject = mailCampos.Titulo;
                email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = mailCampos.Cuerpo };
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect(mailCampos.Server, mailCampos.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(mailCampos.UserName, mailCampos.Password);
                smtp.Send(email);
                smtp.Disconnect(true);

                await WriteBitacora(mailCampos.UserId, mailCampos.OrgId,
                    $"Se envio un Email a {mailCampos.Para} Titulo {mailCampos.Titulo}",
                    mailCampos.Corporativo, false);
                apiRespuesta.Exito = true;
                return apiRespuesta;
            }
            catch (Exception ex)
            {
                apiRespuesta.MsnError.Add(ex.Message);
                var text = $"Hubo un error al enviar MAIL {ex} Para {mailCampos.Para} ";
                text += $"Titulo {mailCampos.Titulo} ";
                await WriteBitacora(mailCampos.UserId, mailCampos.OrgId, text,
                    mailCampos.Corporativo, true);
                return apiRespuesta;
            }
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                
                throw new InvalidOperationException(
                    $"No pude creear un nuevo usuario de '{nameof(IdentityUser)}'. ");
            }
        }
        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Se requiere soporte de correo electronico!.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }

        public async Task WriteBitacora(string uId, string oId, string d, string corp, bool s)
        {
            try
            {
                if (s)
                {
                    var bitaLogTemp = MyFunc.MakeLog(uId, oId, d, corp, s);
                    await _appDbContext.LogsBitacora.AddAsync(bitaLogTemp);
                }
                else
                {
                    var bitaTemp = MyFunc.MakeBitacora(uId, oId, d, corp, s);
                    await _appDbContext.Bitacora.AddAsync(bitaTemp);
                }
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var eTxt = $"Se genero un error al intentar escribir en la bitacora {ex}";
                var user = uId ?? "Sin Usuario";
                var org = oId ?? "Sin Organizacion";
                var corporativo = corp ?? "Sin Corporativo"; 
                if (s)
                {
                    var bLT = MyFunc.MakeLog(user, org, eTxt, corporativo, s);
                    await _appDbContext.LogsBitacora.AddAsync(bLT);
                }
                else
                {
                    var bT = MyFunc.MakeBitacora(user, org, eTxt, corporativo, s);
                    await _appDbContext.Bitacora.AddAsync(bT);
                }
                await _appDbContext.SaveChangesAsync();
               
            }
            

        }

    }
}

