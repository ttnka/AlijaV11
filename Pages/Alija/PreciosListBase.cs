using System;
using DashBoard.Data;
using DashBoard.Modelos;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace DashBoard.Pages.Alija
{
	public class PreciosListBase : ComponentBase 
	{
        public const string TBita = "Productos";

        [Inject]
        public Repo<Z280_Producto, ApplicationDbContext> ProductoRepo { get; set; } = default!;

        public List<Z280_Producto> LosProductos { get; set; } = new List<Z280_Producto>();

        [CascadingParameter(Name = "EmpresaActivaAll")]
        public Z100_Org EmpresaActiva { get; set; } = new();

        [Parameter]
        public List<Z100_Org> LasOrgs { get; set; } = new List<Z100_Org>();
        protected List<Z100_Org> LosCorps { get; set; } = new List<Z100_Org>();
        protected List<KeyValuePair<string, string>> PreciosTipo { get; set; } = new List<KeyValuePair<string, string>>(); 

        public RadzenDataGrid<Z280_Producto>? ProductoGrid { get; set; } = new RadzenDataGrid<Z280_Producto>();

        protected bool Primera { get; set; } = true;
        protected bool Leyendo { get; set; } = false;
        protected bool Editando { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            if (Primera)
            {
                Primera = false;
                await LeerProductos();
                
            }

            await Leer();
        }

        protected async Task Leer()
        {
            try
            {
                await LeerPreciosTipo();
                await LeerLosCorps();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerProductos()
        {
            try
            {
                if (ElUser.Nivel == 6)
                {
                    LosProductos = (await ProductoRepo.GetAll()).ToList();
                }
                else
                {
                    LosProductos = (await ProductoRepo.Get(x=>x.Status == true && x.Estado == 1)).ToList();
                }
                    
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos de los Productos, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerLosCorps()
        {
            try
            {
                if(LasOrgs.Any())
                {
                    Z100_Org Todas = new Z100_Org() { OrgId = "All", Comercial = "General Todos los clientes" };
                    IEnumerable<Z100_Org> resp = LasOrgs.Where(x => x.Tipo == "Administracion" && x.Estado == 1 && x.Status == true);
                    LosCorps.Add(Todas);
                    if(resp.Any())
                        LosCorps.AddRange(resp.OrderBy(x => x.Comercial));
                    
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar Leer y poblar la empresa que factura {TBita} {ex}",
                        Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task LeerPreciosTipo()
        {
            try
            {
                List<string> pt = Constantes.TiposdePrecios.Split(",").ToList();
                foreach(var p in pt )
                {
                    PreciosTipo.Add(new KeyValuePair<string, string>(p, p));
                }
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                        $"Error al intentar Leer y poblar tipos de precios {TBita} {ex}",
                        Corporativo, ElUser.OrgId);
                await LogAll(LogT);
            }
        }

        protected async Task<ApiRespuesta<Z280_Producto>> Servicio(ServiciosTipos tipo, Z280_Producto prod)
        {
            ApiRespuesta<Z280_Producto> resp = new()
            {
                Exito = false,
                Data = prod
            };

            try
            {
                if (prod != null)
                {
                    if (tipo == ServiciosTipos.Insert)
                    {
                        prod.ProductoId = Guid.NewGuid().ToString();

                        Z280_Producto prodInsert = await ProductoRepo.Insert(prod);
                        if (prodInsert != null)
                        {
                            resp.Exito = true;
                            resp.Data = prodInsert;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Inserto el registro Producto {prod.Titulo}");
                        }
                        return resp;
                    }
                    else if (tipo == ServiciosTipos.Update)
                    {
                        Z280_Producto prodUpdate = await ProductoRepo.Update(prod);
                        if (prodUpdate != null)
                        {
                            resp.Exito = true;
                            resp.Data = prodUpdate;
                        }
                        else
                        {
                            resp.MsnError.Add($"No se Actualizo el registro PRODUCTO {prod.Titulo}");
                            resp.Exito = false;
                        }
                        return resp;
                    }
                }
                resp.MsnError.Add($"Ningua operacion se realizo! {TBita}");
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

