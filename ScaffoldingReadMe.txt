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









    
