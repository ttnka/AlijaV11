using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashBoard.Migrations
{
    /// <inheritdoc />
    public partial class Sig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campos",
                columns: table => new
                {
                    CampoId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FolioId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AgenteAduanal = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Pedimento = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Factura = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mercancia = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tractor = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Caja1 = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Caja2 = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Placas = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TractorTipo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Transportista = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Maniobra = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Despachador = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CartaPorte = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SelloRemovido = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SelloColocado = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Chofer = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Identificacion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cuadrilla = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Obs = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campos", x => x.CampoId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Concpetos",
                columns: table => new
                {
                    ConceptoId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FolioId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Renglon = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Producto = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descuento = table.Column<int>(type: "int", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Obs = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concpetos", x => x.ConceptoId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Campos");

            migrationBuilder.DropTable(
                name: "Concpetos");
        }
    }
}
