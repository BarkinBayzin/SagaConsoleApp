using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaConsoleApp_v2.Migrations
{
    /// <inheritdoc />
    public partial class addNewColumnToWorkflowSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkflowReasons",
                table: "OfferWorkflowHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkflowType",
                table: "OfferWorkflowHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkflowReasons",
                table: "OfferWorkflowHistories");

            migrationBuilder.DropColumn(
                name: "WorkflowType",
                table: "OfferWorkflowHistories");
        }
    }
}
