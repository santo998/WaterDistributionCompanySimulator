using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utiles.Migrations;

/// <inheritdoc />
public partial class InicializarBD : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		_ = migrationBuilder.CreateTable(
			name: "Nodo",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false),
				CaudalEsperado = table.Column<double>(type: "float", nullable: false),
				IdPadre = table.Column<int>(type: "int", nullable: true),
				Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
				ToleranciaCaudal = table.Column<double>(type: "float", nullable: false)
			},
			constraints: table =>
			{
				_ = table.PrimaryKey("PK_Nodo", x => x.Id);
				_ = table.ForeignKey(
					name: "FK_Nodo_Nodo_IdPadre",
					column: x => x.IdPadre,
					principalTable: "Nodo",
					principalColumn: "Id");
			});

		_ = migrationBuilder.CreateTable(
			name: "Suscripcion",
			columns: table => new
			{
				ChatId = table.Column<string>(type: "nvarchar(450)", nullable: false)
			},
			constraints: table => table.PrimaryKey("PK_Suscripcion", x => x.ChatId));

		_ = migrationBuilder.CreateTable(
			name: "Medicion",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("SqlServer:Identity", "1, 1"),
				Analizada = table.Column<bool>(type: "bit", nullable: false),
				Caudal = table.Column<double>(type: "float", nullable: false),
				FechaUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
				IdNodo = table.Column<int>(type: "int", nullable: false)
			},
			constraints: table =>
			{
				_ = table.PrimaryKey("PK_Medicion", x => x.Id);
				_ = table.ForeignKey(
					name: "FK_Medicion_Nodo_IdNodo",
					column: x => x.IdNodo,
					principalTable: "Nodo",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

		_ = migrationBuilder.CreateTable(
			name: "Perdida",
			columns: table => new
			{
				Id = table.Column<int>(type: "int", nullable: false)
					.Annotation("SqlServer:Identity", "1, 1"),
				FueNotificada = table.Column<bool>(type: "bit", nullable: false),
				IdMedicion = table.Column<int>(type: "int", nullable: false)
			},
			constraints: table =>
			{
				_ = table.PrimaryKey("PK_Perdida", x => x.Id);
				_ = table.ForeignKey(
					name: "FK_Perdida_Medicion_IdMedicion",
					column: x => x.IdMedicion,
					principalTable: "Medicion",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
			});

		_ = migrationBuilder.CreateIndex(
			name: "IX_Medicion_IdNodo",
			table: "Medicion",
			column: "IdNodo");

		_ = migrationBuilder.CreateIndex(
			name: "IX_Nodo_IdPadre",
			table: "Nodo",
			column: "IdPadre");

		_ = migrationBuilder.CreateIndex(
			name: "IX_Perdida_IdMedicion",
			table: "Perdida",
			column: "IdMedicion");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		_ = migrationBuilder.DropTable(
			name: "Perdida");

		_ = migrationBuilder.DropTable(
			name: "Suscripcion");

		_ = migrationBuilder.DropTable(
			name: "Medicion");

		_ = migrationBuilder.DropTable(
			name: "Nodo");
	}
}
