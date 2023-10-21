﻿using System;
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
        public List<Z100_Org> LosCorps { get; set; } = new List<Z100_Org>();

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
                await LeerLosCorps();
            }
            catch (Exception ex)
            {
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                $"Error al intentar Leer datos INICIO, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
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
                await LogRepo.Insert(LogT);
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
                await LogRepo.Insert(LogT);
            }
        }

        protected async Task<ApiRespuesta<Z280_Producto>> Servicio(string tipo, Z280_Producto prod)
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
                    if (tipo == "Insert")
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
                    else if (tipo == "Update")
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
                await LogRepo.Insert(LogT);
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
                Z192_Logs LogT = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error al intentar escribir BITACORA, {TBita},{ex}",
                    Corporativo, ElUser.OrgId);
                await LogRepo.Insert(LogT);
            }

        }
        #endregion


    }
}

