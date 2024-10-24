﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SagaConsoleApp_v2.Data;

#nullable disable

namespace SagaConsoleApp_v2.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241024160410_addNewColumnToWorkflowSaga")]
    partial class addNewColumnToWorkflowSaga
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.CrmOpportunity", b =>
                {
                    b.Property<Guid>("OpportunityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("GhTur")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("OpportunityId");

                    b.ToTable("CrmOpportunities");
                });

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.Offer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Creator")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("GhTur")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDraft")
                        .HasColumnType("bit");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Offers");
                });

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.OfferWorkflowHistory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("OfferId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("WorkflowInstanceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("WorkflowType")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OfferId");

                    b.ToTable("OfferWorkflowHistories");
                });

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.WorkflowInstance", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("StarterFullName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("StarterUserEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("StarterUserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("WorkflowInstances");
                });

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.WorkflowTask", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("AssignDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("AssignedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<Guid>("AssignedId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AssignedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int>("AssignedType")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CompleteDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("State")
                        .HasColumnType("int");

                    b.Property<string>("TaskDescription")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<int>("TaskStatus")
                        .HasColumnType("int");

                    b.Property<string>("TaskTitle")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<Guid>("WorkflowInstanceId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("WorkflowInstanceId");

                    b.ToTable("WorkflowTasks");
                });

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.OfferWorkflowHistory", b =>
                {
                    b.HasOne("SagaConsoleApp_v2.Entities.Offer", "Offer")
                        .WithMany("OfferWorkflowHistories")
                        .HasForeignKey("OfferId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("SagaConsoleApp_v2.Entities.WorkflowReason", "WorkflowReasons", b1 =>
                        {
                            b1.Property<Guid>("OfferWorkflowHistoryId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("int");

                            b1.Property<string>("Reason")
                                .HasColumnType("nvarchar(max)")
                                .HasAnnotation("Relational:JsonPropertyName", "reason");

                            b1.Property<int>("StateType")
                                .HasColumnType("int")
                                .HasAnnotation("Relational:JsonPropertyName", "stateType");

                            b1.Property<Guid>("TaskOwnerId")
                                .HasColumnType("uniqueidentifier")
                                .HasAnnotation("Relational:JsonPropertyName", "taskOwnerId");

                            b1.HasKey("OfferWorkflowHistoryId", "Id");

                            b1.ToTable("OfferWorkflowHistories");

                            b1.ToJson("WorkflowReasons");

                            b1.WithOwner()
                                .HasForeignKey("OfferWorkflowHistoryId");
                        });

                    b.Navigation("Offer");

                    b.Navigation("WorkflowReasons");
                });

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.WorkflowInstance", b =>
                {
                    b.HasOne("SagaConsoleApp_v2.Entities.OfferWorkflowHistory", "OfferWorkflowHistory")
                        .WithOne("WorkflowInstance")
                        .HasForeignKey("SagaConsoleApp_v2.Entities.WorkflowInstance", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OfferWorkflowHistory");
                });

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.WorkflowTask", b =>
                {
                    b.HasOne("SagaConsoleApp_v2.Entities.WorkflowInstance", "WorkflowInstance")
                        .WithMany("WorkflowTasks")
                        .HasForeignKey("WorkflowInstanceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("WorkflowInstance");
                });

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.Offer", b =>
                {
                    b.Navigation("OfferWorkflowHistories");
                });

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.OfferWorkflowHistory", b =>
                {
                    b.Navigation("WorkflowInstance")
                        .IsRequired();
                });

            modelBuilder.Entity("SagaConsoleApp_v2.Entities.WorkflowInstance", b =>
                {
                    b.Navigation("WorkflowTasks");
                });
#pragma warning restore 612, 618
        }
    }
}