using System;
using System.Net.Mail;
using System.Security.Policy;
using System.Text;
using DashBoard.Data;
using DashBoard.Modelos;
using DashBoard.Pages.Zuver;
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

        public async Task<ApiRespuesta<AddUser>>CrearNewAcceso(AddUser data)
        {
            ApiRespuesta<AddUser> resultado = new();
            try
            {
                IdentityUser user = CreateUser();
                await _userStore.SetUserNameAsync(user, data.Mail, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, data.Mail, CancellationToken.None);
                IdentityResult result = await _userManager.CreateAsync(user, data.Pass);
                if (result.Succeeded)
                {
                    string userId = await _userManager.GetUserIdAsync(user);

                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    string url = ($"{Constantes.ConfirmarMailTxt}{userId}&code={code}");

                    data.UserId = userId;
                    data.Url = url;
                    resultado.Exito = true;
                    resultado.Data = data;
                }
                else
                { 
                    resultado.Exito = false;
                    resultado.MsnError.Add("No se creo al nuevo accceso no hay explicacion");
                    resultado.MsnError.AddRange(result.Errors
                        .Select(error => error.Description));
                }
            }
            catch (Exception ex)
            {
                resultado.MsnError.Add($"Error, No se creo el nuevo acceso al sistema {ex}");
                throw ;
            }
            return resultado;
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
    }
}

