Support for ASP.NET Core Identity was added to your project.

For setup and configuration information, see https://go.microsoft.com/fwlink/?linkid=2116645.


AlijaV11
Applicacion de mi memes Alijadores
esta en la base de datos Omins - Austeridad


como unir 2 tablas 
resp = LosUsers.Where(user =>
                                    LasAlijadoras.Select(org => org.OrgId).Contains(user.OrgId)).ToList();

Formato de dinero con <div text-align: right;
@($"{String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", @ConceptoGrid.View.Select(x => x.Importe).Sum())}")

por grupos 
LasOrgsCorp = LasOrgs.Where(x => x.Estado == 1).GroupBy(x => x.Corporativo).Select(x => x.First()).ToList();


async Task Uno() {
}




@using Microsoft.EntityFrameworkCore
@using System.Linq

@inherits DbContextPage

@inject DialogService DialogService

<RadzenDataGrid @ref="ordersGrid" AllowFiltering="true" FilterPopupRenderMode="PopupRenderMode.OnDemand" AllowPaging="true" PageSize="5" AllowSorting="true"
                Data="@orders" TItem="Order">
    <Columns>
        <RadzenDataGridColumn Width="50px" TItem="Order" Title="#" Filterable="false" Sortable="false" TextAlign="TextAlign.Center">
            <Template Context="data">
                @(orders.IndexOf(data) + 1)
            </Template>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="Order" Property="Employee.LastName" Title="Employee">
            <Template Context="order">
                <RadzenImage Path="@order.Employee?.Photo" style="width: 40px; height: 40px; border-radius: 8px; margin-right: 8px; float: left;" />
                <RadzenText TextStyle="TextStyle.Subtitle2" class="mb-0">@order.Employee?.FirstName @order.Employee?.LastName</RadzenText>
                <RadzenText TextStyle="TextStyle.Caption">@order.Customer?.CompanyName</RadzenText>
            </Template>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="Order" Property="OrderDate" Title="Order Date" FormatString="{0:d}" />
        <RadzenDataGridColumn TItem="Order" Property="Freight" Title="Freight">
            <Template Context="order">
                @String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", order.Freight)
            </Template>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="Order" Property="ShipCountry" Title="ShipCountry" />
        <RadzenDataGridColumn Width="160px" TItem="Order" Property="OrderID" Title="Order Details">
            <Template Context="data">
                <RadzenButton ButtonStyle="ButtonStyle.Info" Variant="Variant.Flat" Shade="Shade.Lighter" Icon="info" class="m-1" Click=@(() => OpenOrder(data.OrderID)) Text="@data.OrderID.ToString()" />
            </Template>
        </RadzenDataGridColumn>
    </Columns>
</RadzenDataGrid>

@code {
    RadzenDataGrid<Order> ordersGrid;
    IList<Order> orders;

    async Task OpenOrder(int orderId)
    {
        await DialogService.OpenAsync<DialogCardPage>($"Order {orderId}",
              new Dictionary<string, object>() { { "OrderID", orderId } },
              new DialogOptions() { Width = "700px", Height = "520px" });
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        orders = dbContext.Orders.Include("Customer").Include("Employee").ToList();
    }
}




@using RadzenBlazorDemos.Data
@using RadzenBlazorDemos.Models.Northwind
@using Microsoft.EntityFrameworkCore

@inherits DbContextPage

<RadzenButton ButtonStyle="ButtonStyle.Success" Icon="add_circle_outline" class="mt-2 mb-4" Text="Add New Order" Click="@InsertRow" Disabled=@(orderToInsert != null || orderToUpdate != null) />
<RadzenDataGrid @ref="ordersGrid" AllowAlternatingRows="false" AllowFiltering="true" AllowPaging="true" PageSize="5" AllowSorting="true" EditMode="DataGridEditMode.Single"
            Data="@orders" TItem="Order" RowUpdate="@OnUpdateRow" RowCreate="@OnCreateRow" Sort="@Reset" Page="@Reset" Filter="@Reset" ColumnWidth="200px">
    <Columns>
        <RadzenDataGridColumn TItem="Order" Property="OrderID" Title="Order ID" Width="120px" />
        <RadzenDataGridColumn TItem="Order" Property="Customer.CompanyName" Title="Customer" Width="280px" >
            <EditTemplate Context="order">
                <RadzenDropDown @bind-Value="order.CustomerID" Data="@customers" TextProperty="CompanyName" ValueProperty="CustomerID" Style="width:100%; display: block;" />
            </EditTemplate>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="Order" Property="Employee.LastName" Title="Employee" Width="220px">
            <Template Context="order">
                <RadzenImage Path="@order.Employee?.Photo" style="width: 32px; height: 32px; border-radius: 16px; margin-right: 6px;" />
                @order.Employee?.FirstName @order.Employee?.LastName
            </Template>
            <EditTemplate Context="order">
                <RadzenDropDown @bind-Value="order.EmployeeID" Data="@employees" ValueProperty="EmployeeID" Style="width:100%; display: block;">
                    <Template>
                        <RadzenImage Path="@context.Photo" style="width: 20px; height: 20px; border-radius: 16px; margin-right: 6px;" />
                        @context.FirstName @context.LastName
                    </Template>
                </RadzenDropDown>
            </EditTemplate>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="Order" Property="OrderDate" Title="Order Date" Width="200px">
            <Template Context="order">
                @String.Format("{0:d}", order.OrderDate)
            </Template>
            <EditTemplate Context="order">
                <RadzenDatePicker @bind-Value="order.OrderDate" Style="width:100%" />
            </EditTemplate>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="Order" Property="Freight" Title="Freight">
            <Template Context="order">
                @String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", order.Freight)
            </Template>
            <EditTemplate Context="order">
                <RadzenNumeric @bind-Value="order.Freight" Style="width:100%" />
            </EditTemplate>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="Order" Property="ShipName" Title="Ship Name">
            <EditTemplate Context="order">
                <RadzenTextBox @bind-Value="order.ShipName" Style="width:100%; display: block" Name="ShipName" />
                <RadzenRequiredValidator Text="ShipName is required" Component="ShipName" Popup="true" />
            </EditTemplate>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="Order" Context="order" Filterable="false" Sortable="false" TextAlign="TextAlign.Right" Width="156px">
            <Template Context="order">
                <RadzenButton Icon="edit" ButtonStyle="ButtonStyle.Light" Variant="Variant.Flat" Size="ButtonSize.Medium" Click="@(args => EditRow(order))" @onclick:stopPropagation="true">
                </RadzenButton>
                <RadzenButton ButtonStyle="ButtonStyle.Danger" Icon="delete" Variant="Variant.Flat" Shade="Shade.Lighter" Size="ButtonSize.Medium" class="my-1 ms-1" Click="@(args => DeleteRow(order))"  @onclick:stopPropagation="true">
                </RadzenButton>
            </Template>
            <EditTemplate Context="order">
                <RadzenButton Icon="check" ButtonStyle="ButtonStyle.Success" Variant="Variant.Flat" Size="ButtonSize.Medium" Click="@((args) => SaveRow(order))">
                </RadzenButton>
                <RadzenButton Icon="close" ButtonStyle="ButtonStyle.Light" Variant="Variant.Flat" Size="ButtonSize.Medium" class="my-1 ms-1" Click="@((args) => CancelEdit(order))">
                </RadzenButton>
                <RadzenButton ButtonStyle="ButtonStyle.Danger" Icon="delete" Variant="Variant.Flat" Shade="Shade.Lighter" Size="ButtonSize.Medium" class="my-1 ms-1" Click="@(args => DeleteRow(order))">
                </RadzenButton>
            </EditTemplate>
        </RadzenDataGridColumn>
    </Columns>
</RadzenDataGrid>

@code {
    RadzenDataGrid<Order> ordersGrid;
    IEnumerable<Order> orders;
    IEnumerable<Customer> customers;
    IEnumerable<Employee> employees;

    Order orderToInsert;
    Order orderToUpdate;

    void Reset()
    {
        orderToInsert = null;
        orderToUpdate = null;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        customers = dbContext.Customers;
        employees = dbContext.Employees;

        orders = dbContext.Orders.Include("Customer").Include("Employee");
    }

    async Task EditRow(Order order)
    {
        orderToUpdate = order;
        await ordersGrid.EditRow(order);
    }

    void OnUpdateRow(Order order)
    {
        Reset();

        dbContext.Update(order);

        dbContext.SaveChanges();
    }

    async Task SaveRow(Order order)
    {
        await ordersGrid.UpdateRow(order);
    }

    void CancelEdit(Order order)
    {
        Reset();

        ordersGrid.CancelEditRow(order);

        var orderEntry = dbContext.Entry(order);
        if (orderEntry.State == EntityState.Modified)
        {
            orderEntry.CurrentValues.SetValues(orderEntry.OriginalValues);
            orderEntry.State = EntityState.Unchanged;
        }
    }

    async Task DeleteRow(Order order)
    {
        Reset();

        if (orders.Contains(order))
        {
            dbContext.Remove<Order>(order);

            dbContext.SaveChanges();

            await ordersGrid.Reload();
        }
        else
        {
            ordersGrid.CancelEditRow(order);
            await ordersGrid.Reload();
        }
    }

    async Task InsertRow()
    {
        orderToInsert = new Order();
        await ordersGrid.InsertRow(orderToInsert);
    }

    void OnCreateRow(Order order)
    {
        dbContext.Add(order);

        dbContext.SaveChanges();
        
        orderToInsert = null;
    }
}


public class Repo<TEntity, TDataContext> : ApiFiltroGet<TEntity>
        where TEntity : class
        where TDataContext : DbContext
    {
        protected readonly TDataContext context;
        internal DbSet<TEntity> dbset;

        private readonly ApplicationDbContext _appDbContext;
        MyFunc myFunc = new();
        
        public Repo(TDataContext dataContext,
            ApplicationDbContext appDbContext)
        {
            context = dataContext;
            dbset = context.Set<TEntity>();
            _appDbContext = appDbContext;
        }

        public virtual async Task<TEntity> Insert(TEntity entity)
        {
            try
            {
                await dbset.AddAsync(entity);
                await context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                string etxt = $"Error al intentar INSERTAR un registro REPO<> {ex}";
                var logTmp = myFunc.MakeLog("SistemaUserID", "SistemaOrgId", etxt,
                    "SistemaCorp", "Sistema");
                    await _appDbContext.LogsBitacora.AddAsync(logTmp);
                await _appDbContext.SaveChangesAsync();
                throw;
            }
        }        
    }
}


 
public class Z100_Org
	{
        [Key]
        [StringLength(50)]
        public string OrgId { get; set; } = "";
        [StringLength(15)]
        public string Rfc { get; set; } = "";
        [StringLength(25)]
        public string Comercial { get; set; } = "";
        [StringLength(75)]
        public string? RazonSocial { get; set; } = "";
        public bool Moral { get; set; } = true;
        
        public string? NumCliente { get; set; } = "";
        [StringLength(15)]
        public string Tipo { get; set; } = "Cliente";
        [StringLength(50)]
        public string Corporativo { get; set; } = "All";
        
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;

        public string ComercialRfc => Comercial + " " + Rfc;
        
    }

public class Z110_User
	{
        [Key]
        [StringLength(50)]
        public string UserId { get; set; } = "";
        [StringLength(25)]
        public string Nombre { get; set; } = "";
        [StringLength(25)]
        public string Paterno { get; set; } = "";
        [StringLength(25)]
        public string? Materno { get; set; }
        public int Nivel { get; set; } = 1;
        [StringLength(50)]
        public string OrgId { get; set; } = "";
        [StringLength(75)]
        public string OldEmail { get; set; } = "";
        
        public int Estado { get; set; } = 2;
        public bool Status { get; set; } = true;
        public string Completo => Nombre + " " + Paterno + " " + Materno;
    }









@inherits FileListBase
@inject NotificationService NS
@using DashBoard.Modelos


@if (LosConceptos != null && !Leyendo)
{
    <RadzenDataGrid @ref="ConceptoGrid"
                    AllowFiltering="true" AllowPaging="true" PageSize="50"
                    AllowSorting="true" AllowColumnResize="true"
                    ExpandMode="DataGridExpandMode.Single" AllowGrouping="false"
                    EditMode="DataGridEditMode.Single" AllowColumnPicking="true"
                    Data="@LosConceptos" TItem="Z210_Concepto"
                    RowUpdate="@OnUpdateRow" RowCreate="@OnCreateRow" EmptyText="No hay registros">

        <HeaderTemplate>
            @if (ElUser.Nivel > 4 && EmpresaActiva.OrgId.Length > 30)
            {
                @if (ElFolio.Estado < 2 || ElUser.Nivel > 5)
                {
                    <RadzenButton Icon="plus" style="margin-bottom: 10px"
                                  ButtonStyle="ButtonStyle.Info" Click="InsertRow">
                        Agregar conceptos
                    </RadzenButton>
                }
            }
            else if (EmpresaActiva.OrgId.Length < 30)
            {
                <RadzenLabel>Es necesario que seleciones una empresa activa en Informacion>Empresa Activa</RadzenLabel>
            }
            <RadzenButton Icon="refresh" style="margin-bottom: 10px"
                          ButtonStyle="ButtonStyle.Success" Click="LeerConceptos">
                Actualizar
            </RadzenButton>
        </HeaderTemplate>

        <Columns>


            <RadzenDataGridColumn TItem="Z210_Concepto" Title="Id" Filterable="false"
                                  Width="40px">
                <Template Context="datos">
                    <RadzenLabel>@(LosConceptos.IndexOf(datos) + 1)</RadzenLabel>

                </Template>

            </RadzenDataGridColumn>

            <RadzenDataGridColumn TItem="Z210_Concepto" Title="Cantidad" Property="Cantidad"
                                  Filterable="true" Width="100px">
                <Template Context="datos">
                    <div style="text-align: right;">
                        <RadzenLabel>@datos.Cantidad</RadzenLabel>
                    </div>
                </Template>
                <EditTemplate Context="datos">
                    <div style="text-align: right;">
                        <RadzenNumeric Name="Cantidad" @bind-Value="datos.Cantidad"
                                       Style="width: 100%; text-align: right;" ShowUpDown="false" />
                    </div>
                </EditTemplate>
                <FooterTemplate>
                    <RadzenText TextAlign="TextAlign.Right">
                        TOTAL:
                    </RadzenText>
                </FooterTemplate>

            </RadzenDataGridColumn>

            <RadzenDataGridColumn TItem="Z210_Concepto" Title="Producto / Servicio"
                                  Filterable="false" Resizable="true" Width="225px">

                <Template Context="datos">
                    <div style="white-space:pre-wrap; line-height: initial">
                        @if (LosProductos.Any(x => x.ProductoId == datos.Producto))
                        {
                            <RadzenLabel>
                                @LosProductos.FirstOrDefault(x => x.ProductoId == datos.Producto)!.Titulo
                            </RadzenLabel>
                        }
                        else
                        {
                            <RadzenLabel>
                                No hay Producto
                            </RadzenLabel>
                        }
                    </div>
                </Template>

                <EditTemplate Context="datos">
                    <RadzenDropDown Name="Producto" Data=LosProductos @bind-Value=@datos.Producto
                                    ValueProperty="ProductoId" TextProperty="Titulo"
                                    Style="width: 90%;" Change="CalImporte" />
                </EditTemplate>
                <FooterTemplate>
                    <RadzenText TextAlign="TextAlign.Right">
                        @if (LosConceptos.Count > 0)
                        {
                            @($"Conceptos {ConceptoGrid!.View.Count()}")
                        }
                    </RadzenText>
                </FooterTemplate>
            </RadzenDataGridColumn>

            <RadzenDataGridColumn TItem="Z210_Concepto" Title="Precio"
                                  Filterable="false" Resizable="true" Width="150px">

                <Template Context="datos">

                    <div style="text-align: right;">
                        <RadzenLabel>
                            @($"{String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", @datos.Precio)}")

                        </RadzenLabel>
                    </div>
                </Template>

                <EditTemplate Context="datos">
                    <div style="text-align: right;">
                        @if (DicProductos.ContainsKey(datos.Producto))
                        {
                            <RadzenNumeric Name="Precio" @bind-Value="datos.Precio"
                                           Style="width: 70%; text-align: right;" ShowUpDown="false" />

                        }
                        else
                        {
                            <RadzenLabel>
                                @($"{String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", @datos.Precio)}")

                            </RadzenLabel>
                        }
                    </div>
                </EditTemplate>
            </RadzenDataGridColumn>

            <RadzenDataGridColumn TItem="Z210_Concepto" Title="Importe"
                                  Filterable="false" Resizable="true" Width="125px">

                <Template Context="datos">
                    <div style="text-align: right;">

                        <RadzenLabel>
                            @($"{String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", @datos.Importe)}")

                        </RadzenLabel>
                    </div>
                </Template>

                <EditTemplate Context="datos">
                    <div style="text-align: right;">
                        <RadzenLabel>
                            @($"{String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", @datos.Importe)}")

                        </RadzenLabel>
                    </div>
                </EditTemplate>
                <FooterTemplate>
                    @if (LosConceptos.Count > 0)
                    {
                        <RadzenText TextAlign="TextAlign.Right">
                            @($"{String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", @ConceptoGrid!.View.Select(x => x.Importe).Sum())}")

                        </RadzenText>
                    }
                </FooterTemplate>

            </RadzenDataGridColumn>

            <RadzenDataGridColumn TItem="Z210_Concepto" Title="Comentarios"
                                  Filterable="false" Resizable="true" Width="200px">

                <Template Context="datos">
                    <RadzenLabel style="white-space:pre-wrap; line-height: initial">
                        @datos.Obs
                    </RadzenLabel>
                </Template>

                <EditTemplate Context="datos">
                    <RadzenTextArea Name="Identificacion" @bind-Value="datos.Obs" Placeholder="Comentarios" />
                </EditTemplate>
            </RadzenDataGridColumn>


            <RadzenDataGridColumn TItem="Z210_Concepto" Context="sampleBlazorModelsSampleOrder"
                                  Filterable="false" Sortable="false" TextAlign="TextAlign.Center"
                                  Width="250px" Title="Estado">
                <Template Context="datos">
                    @if (ElUser.Nivel > 5 || (datos.Estado < 2 && ElFolio.Estado < 2))
                    {
                        <RadzenButton Icon="edit" ButtonStyle="ButtonStyle.Secondary"
                                      Class="m-1" Click="@((args) => EditRow(datos))" Visible="@(!Editando)" />
                    }

                    @if (datos.Status)
                    {
                        <b>Activo</b>
                    }
                    else
                    {
                        <b>Suspendido</b>
                    }


                </Template>

                <EditTemplate Context="datos">

                    @if (datos.Estado > 0 && ElUser.Nivel > 4 && ElFolio.Estado < 2)
                    {
                        <div>
                            <RadzenLabel> Borrar este registro? </RadzenLabel><br />
                            <RadzenSelectBar @bind-Value=@datos.Status TValue="bool">
                                <Items>
                                    <RadzenSelectBarItem Text="No" Value="true" />
                                    <RadzenSelectBarItem Text="Si" Value="false" />
                                </Items>
                            </RadzenSelectBar><br />
                        </div>
                    }

                    <RadzenButton Icon="check" ButtonStyle="ButtonStyle.Success"
                                  Class="m-1" Click="@((args) => SaveRow(datos))" />


                    <RadzenButton Icon="close" ButtonStyle="ButtonStyle.Danger" Class="m-1"
                                  Click="@((args) => CancelEdit(datos))" />
                </EditTemplate>

            </RadzenDataGridColumn>
        </Columns>
    </RadzenDataGrid>
}
else
{
    <div class="spinner">

    </div>
}

@code
{

    void CalImporte()
    {
        if (ConceptoToInsert.Producto.Length > 15)
        {
            if (LosProductos.Any(x => x.ProductoId == ConceptoToInsert.Producto))
            {
                ConceptoToInsert.Precio = LosProductos.FirstOrDefault(x =>
                                                x.ProductoId == ConceptoToInsert.Producto)!.Precio;
            }
        }
        if (ConceptoToInsert.Cantidad > 0 && ConceptoToInsert.Precio > 0)
        {
            ConceptoToInsert.Importe = ConceptoToInsert.Cantidad * ConceptoToInsert.Precio;
        }
    }


    void Cancelar()
    {
        //OrgNew = new();
        ConceptoToInsert = new();
    }

    Z210_Concepto ConceptoToInsert = new();

    async Task EditRow(Z210_Concepto concepto)
    {

        await ConceptoGrid!.EditRow(concepto);
        Editando = !Editando;
    }

    async void OnUpdateRow(Z210_Concepto concepto)
    {
        try
        {
            concepto.Importe = concepto.Cantidad * concepto.Precio;

            if (concepto == ConceptoToInsert) ConceptoToInsert = null!;

            Editando = !Editando;
            ApiRespuesta<Z210_Concepto> resultado = await Servicio(ServiciosTipos.Update, concepto);

            Z280_Producto prod = LosProductos.Any(x => x.ProductoId == concepto.Producto) ?
                LosProductos.FirstOrDefault(x => x.ProductoId == concepto.Producto)! : new();
            var imp = prod.Precio * concepto.Cantidad;
            string txt = $"{TBita}, Folio: {ElFolio.FolioId} Renglon: {concepto.Renglon},";
            txt += $"cantidad: {concepto.Cantidad} producto: {prod.Clave} {prod.Titulo}, ";
            txt += $"precio: {prod.Precio}, importe: {imp}";
            txt += string.IsNullOrEmpty(concepto.Obs) ? "" : $"Comentarios: {concepto.Obs}";

            if (resultado.Exito)
            {
                ShowNotification(ElMsn("info", "Actualizo",
                    $"Se actualizo la info de Concpetos del Folio {ElFolio.FolioNum} {prod.Titulo}", 0));
                txt = $"Se actualizo la informacion " + txt;
                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId, txt,
                    Corporativo, ElUser.OrgId);
                await BitacoraAll(bitaTemp);
                await Servicio(ServiciosTipos.Importe, concepto);

            }
            else
            {
                string etxt = $"Error No Se actualizo la info de ";
                foreach (var e in resultado.MsnError)
                { etxt += $", {e}"; }

                ShowNotification(ElMsn("Error", "Error", etxt, 0));
                txt = $"Error, No se actualizo un registro del concepto {txt} {etxt}";

                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId, txt,
                    Corporativo, ElUser.OrgId);
                await LogAll(logTemp);

            }
        }
        catch (Exception ex)
        {
            Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error, No fue posible actualizar la info de Concepto, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
            await LogAll(logTemp);
        }
        await ConceptoGrid!.Reload();

    }

    async Task SaveRow(Z210_Concepto concepto)
    {
        await ConceptoGrid!.UpdateRow(concepto);
    }

    void CancelEdit(Z210_Concepto concepto)
    {
        if (concepto == ConceptoToInsert) ConceptoToInsert = null!;

        Editando = !Editando;
        ConceptoGrid!.CancelEditRow(concepto);
    }

    async Task InsertRow()
    {
        ConceptoToInsert = new Z210_Concepto()
        {
            FolioId = ElFolio.FolioId,
            Cantidad = 1,
            Estado = 0
        };

        Editando = !Editando;
        await ConceptoGrid!.InsertRow(ConceptoToInsert);

    }
    async void OnCreateRow(Z210_Concepto concepto)
    {
        try
        {
            concepto.Importe = concepto.Cantidad * concepto.Precio;
            if (concepto == ConceptoToInsert) ConceptoToInsert = null!;

            Editando = !Editando;
            ApiRespuesta<Z210_Concepto> resultado = await Servicio(ServiciosTipos.Insert, concepto);


            Z280_Producto prod = LosProductos.Any(x => x.ProductoId == concepto.Producto) ?
                LosProductos.FirstOrDefault(x => x.ProductoId == concepto.Producto)! : new();
            var imp = prod.Precio * concepto.Cantidad;
            string txt = $"{TBita}, Folio: {ElFolio.FolioId} Renglon: {concepto.Renglon},";
            txt += $"cantidad: {concepto.Cantidad} producto: {prod.Clave} {prod.Titulo}, ";
            txt += $"precio: {prod.Precio}, importe: {imp}";
            txt += string.IsNullOrEmpty(concepto.Obs) ? "" : $"Comentarios: {concepto.Obs}";

            if (resultado.Exito)
            {
                ShowNotification(ElMsn("Exito", "Nuevo Concepto",
                    $"Se creo un nuevo registro de CONCEPTO folio: {ElFolio.FolioNum} producto: {prod.Titulo}", 0));

                Z190_Bitacora bitaTemp = MyFunc.MakeBitacora(ElUser.UserId, ElUser.OrgId, txt,
                    Corporativo, ElUser.OrgId);
                await BitacoraAll(bitaTemp);
                await Servicio(ServiciosTipos.Importe, concepto);


            }
            else
            {
                string etxt = $"Error NO se creo el nuevo registro de CONCEPTO ";
                foreach (var e in resultado.MsnError)
                { etxt += $", {e}"; }

                ShowNotification(ElMsn("Error", "Error", etxt, 0));
                txt = $"{TBita}, Error, No se creo un nuevo registro de Transportista " + txt + ", ";
                txt += etxt;
                Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId, txt,
                    Corporativo, ElUser.OrgId);
                await LogAll(logTemp);

            }

        }
        catch (Exception ex)
        {
            Z192_Logs logTemp = MyFunc.MakeLog(ElUser.UserId, ElUser.OrgId,
                    $"Error, No fue posible crear el registro concepto, {TBita}, {ex}",
                    Corporativo, ElUser.OrgId);
            await LogAll(logTemp);
        }
        await ConceptoGrid!.Reload();

    }

    public void ShowNotification(NotificationMessage message)
    {
        NS.Notify(message);
    }
    /*



     @($"{String.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", @ConceptoGrid.View.Select(x => x.Importe).Sum())}")


    */

}

