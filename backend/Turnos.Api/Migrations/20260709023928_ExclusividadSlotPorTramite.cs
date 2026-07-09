using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.Api.Migrations
{
    /// <inheritdoc />
    public partial class ExclusividadSlotPorTramite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Turnos_FechaHora_SoloConfirmados",
                table: "Turnos");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_FechaHora_TipoTramite_SoloConfirmados",
                table: "Turnos",
                columns: new[] { "FechaHora", "TipoTramite" },
                unique: true,
                filter: "\"Estado\" = 'Confirmado'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Turnos_FechaHora_TipoTramite_SoloConfirmados",
                table: "Turnos");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_FechaHora_SoloConfirmados",
                table: "Turnos",
                column: "FechaHora",
                unique: true,
                filter: "\"Estado\" = 'Confirmado'");
        }
    }
}
