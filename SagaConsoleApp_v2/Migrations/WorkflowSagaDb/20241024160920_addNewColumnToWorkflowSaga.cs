using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaConsoleApp_v2.Migrations.WorkflowSagaDb
{
    /// <inheritdoc />
    public partial class addNewColumnToWorkflowSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OfferWorkflowHistoryId",
                table: "WorkflowSagaState",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfferWorkflowHistoryId",
                table: "WorkflowSagaState");
        }
    }
}
