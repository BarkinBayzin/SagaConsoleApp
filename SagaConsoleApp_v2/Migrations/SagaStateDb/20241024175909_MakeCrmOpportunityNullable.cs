using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaConsoleApp_v2.Migrations.SagaStateDb
{
    /// <inheritdoc />
    public partial class MakeCrmOpportunityNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrmOpportunity",
                table: "OvercapacitySagaStates");

            migrationBuilder.AlterColumn<string>(
                name: "FailureReason",
                table: "OvercapacitySagaStates",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentState",
                table: "OvercapacitySagaStates",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<Guid>(
                name: "CrmOpportunityId",
                table: "OvercapacitySagaStates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "InitialOfferId",
                table: "OvercapacitySagaStates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrmOpportunityId",
                table: "OvercapacitySagaStates");

            migrationBuilder.DropColumn(
                name: "InitialOfferId",
                table: "OvercapacitySagaStates");

            migrationBuilder.AlterColumn<string>(
                name: "FailureReason",
                table: "OvercapacitySagaStates",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentState",
                table: "OvercapacitySagaStates",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AddColumn<string>(
                name: "CrmOpportunity",
                table: "OvercapacitySagaStates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
