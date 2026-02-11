using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PointOfSale.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    idCategory = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: true),
                    registrationDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Category__79D361B6930E16FF", x => x.idCategory);
                });

            migrationBuilder.CreateTable(
                name: "CorrelativeNumber",
                columns: table => new
                {
                    idCorrelativeNumber = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lastNumber = table.Column<int>(type: "integer", nullable: true),
                    quantityDigits = table.Column<int>(type: "integer", nullable: true),
                    management = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    dateUpdate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Correlat__D71CDFB02EFC51E4", x => x.idCorrelativeNumber);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    idMenu = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: true),
                    idMenuParent = table.Column<int>(type: "integer", nullable: true),
                    icon = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: true),
                    controller = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: true),
                    pageAction = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: true),
                    registrationDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Menu__C26AF48328C80B96", x => x.idMenu);
                    table.ForeignKey(
                        name: "FK__Menu__idMenuPare__108B795B",
                        column: x => x.idMenuParent,
                        principalTable: "Menu",
                        principalColumn: "idMenu");
                });

            migrationBuilder.CreateTable(
                name: "Negocio",
                columns: table => new
                {
                    idNegocio = table.Column<int>(type: "integer", nullable: false),
                    urlLogo = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    nombreLogo = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    numeroDocumento = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    nombre = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    correo = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    direccion = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    telefono = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    porcentajeImpuesto = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    simboloMoneda = table.Column<string>(type: "character varying(5)", unicode: false, maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Negocio__70E1E107B97CE30F", x => x.idNegocio);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    idRol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(30)", unicode: false, maxLength: 30, nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: true),
                    registrationDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Rol__3C872F76804F2E15", x => x.idRol);
                });

            migrationBuilder.CreateTable(
                name: "TypeDocumentSale",
                columns: table => new
                {
                    idTypeDocumentSale = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: true),
                    registrationDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TypeDocu__18211B893F81F3B8", x => x.idTypeDocumentSale);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    idProduct = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    barCode = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    brand = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    idCategory = table.Column<int>(type: "integer", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    photo = table.Column<byte[]>(type: "bytea", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: true),
                    registrationDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Product__5EEC79D18F8E118B", x => x.idProduct);
                    table.ForeignKey(
                        name: "FK__Product__idCateg__22AA2996",
                        column: x => x.idCategory,
                        principalTable: "Category",
                        principalColumn: "idCategory");
                });

            migrationBuilder.CreateTable(
                name: "RolMenu",
                columns: table => new
                {
                    idRolMenu = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idRol = table.Column<int>(type: "integer", nullable: true),
                    idMenu = table.Column<int>(type: "integer", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: true),
                    registrationDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RolMenu__CD2045D86DACA6AF", x => x.idRolMenu);
                    table.ForeignKey(
                        name: "FK__RolMenu__idMenu__182C9B23",
                        column: x => x.idMenu,
                        principalTable: "Menu",
                        principalColumn: "idMenu");
                    table.ForeignKey(
                        name: "FK__RolMenu__idRol__173876EA",
                        column: x => x.idRol,
                        principalTable: "Rol",
                        principalColumn: "idRol");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    idUsers = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    idRol = table.Column<int>(type: "integer", nullable: true),
                    password = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    photo = table.Column<byte[]>(type: "bytea", nullable: true),
                    isActive = table.Column<bool>(type: "boolean", nullable: true),
                    registrationDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__981CF2B10C1B1086", x => x.idUsers);
                    table.ForeignKey(
                        name: "FK__Users__idRol__1BFD2C07",
                        column: x => x.idRol,
                        principalTable: "Rol",
                        principalColumn: "idRol");
                });

            migrationBuilder.CreateTable(
                name: "Sale",
                columns: table => new
                {
                    idSale = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    saleNumber = table.Column<string>(type: "character varying(6)", unicode: false, maxLength: 6, nullable: true),
                    idTypeDocumentSale = table.Column<int>(type: "integer", nullable: true),
                    idUsers = table.Column<int>(type: "integer", nullable: true),
                    customerDocument = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: true),
                    clientName = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: true),
                    Subtotal = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    totalTaxes = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    total = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    registrationDate = table.Column<DateTime>(type: "timestamp", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sale__C4AEB198091B7829", x => x.idSale);
                    table.ForeignKey(
                        name: "FK__Sale__idTypeDocu__2B3F6F97",
                        column: x => x.idTypeDocumentSale,
                        principalTable: "TypeDocumentSale",
                        principalColumn: "idTypeDocumentSale");
                    table.ForeignKey(
                        name: "FK__Sale__idUsers__2C3393D0",
                        column: x => x.idUsers,
                        principalTable: "Users",
                        principalColumn: "idUsers");
                });

            migrationBuilder.CreateTable(
                name: "DetailSale",
                columns: table => new
                {
                    idDetailSale = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idSale = table.Column<int>(type: "integer", nullable: true),
                    idProduct = table.Column<int>(type: "integer", nullable: true),
                    brandProduct = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    descriptionProduct = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    categoryProducty = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    total = table.Column<decimal>(type: "numeric(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DetailSa__D072342E21B249E9", x => x.idDetailSale);
                    table.ForeignKey(
                        name: "FK__DetailSal__idSal__300424B4",
                        column: x => x.idSale,
                        principalTable: "Sale",
                        principalColumn: "idSale");
                });

            migrationBuilder.CreateTable(
                name: "SaleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleItems_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "idProduct",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleItems_Sale_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sale",
                        principalColumn: "idSale",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetailSale_idSale",
                table: "DetailSale",
                column: "idSale");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_idMenuParent",
                table: "Menu",
                column: "idMenuParent");

            migrationBuilder.CreateIndex(
                name: "IX_Product_idCategory",
                table: "Product",
                column: "idCategory");

            migrationBuilder.CreateIndex(
                name: "IX_RolMenu_idMenu",
                table: "RolMenu",
                column: "idMenu");

            migrationBuilder.CreateIndex(
                name: "IX_RolMenu_idRol",
                table: "RolMenu",
                column: "idRol");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_idTypeDocumentSale",
                table: "Sale",
                column: "idTypeDocumentSale");

            migrationBuilder.CreateIndex(
                name: "IX_Sale_idUsers",
                table: "Sale",
                column: "idUsers");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_ProductId",
                table: "SaleItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleId",
                table: "SaleItems",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_idRol",
                table: "Users",
                column: "idRol");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrelativeNumber");

            migrationBuilder.DropTable(
                name: "DetailSale");

            migrationBuilder.DropTable(
                name: "Negocio");

            migrationBuilder.DropTable(
                name: "RolMenu");

            migrationBuilder.DropTable(
                name: "SaleItems");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Sale");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "TypeDocumentSale");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Rol");
        }
    }
}
