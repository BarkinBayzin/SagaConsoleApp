using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaConsoleApp_v2.Migrations.SagaStateDb
{
    /// <inheritdoc />
    public partial class updateCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpgradeOffer",
                table: "OvercapacitySagaStates");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferId",
                table: "OvercapacitySagaStates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfferId",
                table: "OvercapacitySagaStates");

            migrationBuilder.AddColumn<string>(
                name: "UpgradeOffer",
                table: "OvercapacitySagaStates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
