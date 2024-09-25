using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaConsoleApp.Migrations.SagaStateDb
{
    /// <inheritdoc />
    public partial class addHasKeyForCorrelactionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OverCapacityUpgradeSagaStates",
                table: "OverCapacityUpgradeSagaStates");

            migrationBuilder.RenameTable(
                name: "OverCapacityUpgradeSagaStates",
                newName: "OverCapacityUpgradeSagaState");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "OverCapacityUpgradeSagaState",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OverCapacityUpgradeSagaState",
                table: "OverCapacityUpgradeSagaState",
                column: "CorrelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OverCapacityUpgradeSagaState",
                table: "OverCapacityUpgradeSagaState");

            migrationBuilder.RenameTable(
                name: "OverCapacityUpgradeSagaState",
                newName: "OverCapacityUpgradeSagaStates");

            migrationBuilder.AlterColumn<string>(
                name: "ErrorMessage",
                table: "OverCapacityUpgradeSagaStates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OverCapacityUpgradeSagaStates",
                table: "OverCapacityUpgradeSagaStates",
                column: "CorrelationId");
        }
    }
}
