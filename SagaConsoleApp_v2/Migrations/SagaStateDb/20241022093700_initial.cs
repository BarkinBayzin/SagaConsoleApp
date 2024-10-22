using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaConsoleApp_v2.Migrations.SagaStateDb
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OvercapacitySagaStates",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    GhTur = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CrmOpportunity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpgradeOffer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvercapacitySagaStates", x => x.CorrelationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OvercapacitySagaStates");
        }
    }
}
