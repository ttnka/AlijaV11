using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;

namespace DashBoard.Pages.Sistema
{
	public class ArranqueBase : ComponentBase
	{
        public const string TBita = "Arranque";
        [Inject]
        public Repo<Z100_Org, ApplicationDbContext> OrgsRepo { get; set; } = default!;

        [Inject]
        public IAddUser AddUserRepo { get; set; } = default!;
        [Inject]
        public Repo<Z110_User, ApplicationDbContext> UserRepo { get; set; } = default!;
        [Inject]
        public Repo<Z190_Bitacora, ApplicationDbContext> BitacoraRepo { get; set; } = default!;
        [Inject]
        public Repo<Z192_Logs, ApplicationDbContext> LogRepo { get; set; } = default!;

        [Inject]
        public NavigationManager NM { get; set; } = default!;

        public bool Editando = false;
        protected override async Task OnInitializedAsync()
        {
            var emp1 = await OrgsRepo.Get(x => x.Rfc == Constantes.PgRfc);
            var emp2 = await OrgsRepo.Get(x => x.Rfc == Constantes.SyRfc);
            if (emp1 != null && emp1.Count() > 0 && emp2 != null && emp2.Count() > 0)
            { NM.NavigateTo("/", true); }
        }

        protected async Task RunInicio()
        {
            Editando = true;
            if (Clave.Pass == Constantes.Arranque)
            {
                Editando = false;
                await Creacion();

            }
            Clave.Pass = "";
            NM.NavigateTo("/");
        }
        protected void PassW()
        {
            if (Clave.Pass == Constantes.Arranque)
                Editando = true;
        }
        protected async Task<bool> Creacion()
        {
            var resultado = await OrgsRepo.GetAll();
            if (resultado == null || resultado.Count() > 1) return false;
            try
            {
                List<string> Errores = new();
                string corporativo = Guid.NewGuid().ToString();
                // Genera una nueva organizacion con datos sistema 
                Z100_Org SysOrg = new()
                {
                    OrgId = corporativo,
                    Rfc = Constantes.SyRfc,
                    Comercial = Constantes.SyRazonSocial,
                    RazonSocial = Constantes.SyRazonSocial,
                    Corporativo = corporativo,
                    Estado = Constantes.SyEstado,
                    Status = Constantes.SyStatus,

                    Tipo = "Administracion"
                };
                var newSysOrg = await OrgsRepo.Insert(SysOrg);


                // Genera un nuevo acceso al sistema con un usuario
                AddUser eAddUsuario = new()
                {
                    Mail = Constantes.SyMail,
                    Pass = Constantes.SysPassword,
                    OrgId = newSysOrg.OrgId,
                    Nombre = "El WebMaster",
                    Paterno = Constantes.ElDominio,
                    Materno = "Inc",
                    Corporativo = corporativo,
                    Sistema = true,
                    Nivel = 6
                };

                var userNew = await AddUserRepo.InsertNewUser(eAddUsuario);

                string userGeneradoUserId = "No_regreso_el_Usuario_generado";
                string userGeneradoOrgId = newSysOrg.OrgId;
                if (userNew != null && userNew.Exito == true)
                {
                    userGeneradoUserId = userNew.Data.UsuarioId ?? userGeneradoUserId;
                    userGeneradoOrgId = userNew.Data.OrgId;
                }

                // Genera una organizacion nueva para publico en general
                Z100_Org PgOrg = new()
                {
                    Rfc = Constantes.PgRfc,
                    Comercial = Constantes.PgRazonSocial,
                    RazonSocial = Constantes.PgRazonSocial,
                    Estado = Constantes.PgEstado,
                    Status = Constantes.PgStatus,
                    Corporativo = corporativo,
                    Tipo = "Administracion"
                };
                var newPgOrg = await OrgsRepo.Insert(PgOrg);


                // Genera acceso para publico en general todos 
                AddUser eAddUsuarioPublico = new()
                {
                    Mail = Constantes.DeMailPublico,
                    Pass = Constantes.PasswordMailPublico,
                    OrgId = newPgOrg.OrgId,
                    Nombre = "Publico",
                    Paterno = "General",
                    Materno = "S/F",
                    Corporativo = corporativo,
                    Sistema = true,
                    Nivel = Constantes.NivelPublico
                };

                var userNewPublico = await AddUserRepo.InsertNewUser(eAddUsuarioPublico);


                var bitaTemp = MyFunc.MakeBitacora(userGeneradoUserId, userGeneradoOrgId,
                    $"{TBita}, Se las tablas por primera vez", "All", false);
                await BitacoraRepo.Insert(bitaTemp);
                return true;
            }
            catch (Exception ex)
            {
                var bitaTemp = MyFunc.MakeLog("Sistema"!, "Sistema",
                    $"{TBita}, Error al intentar Arranque de bases de datos {ex}", "All", true);
                await LogRepo.Insert(bitaTemp);
                return false;
            }
        }

        public class LaClave
        {
            public string Pass { get; set; } = "";
        }
        public LaClave Clave { get; set; } = new();
        public MyFunc MyFunc { get; set; } = new();
    }
}

