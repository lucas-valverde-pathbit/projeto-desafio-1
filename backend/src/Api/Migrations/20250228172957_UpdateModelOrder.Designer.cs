﻿// <auto-generated />
using System;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250228172957_UpdateModelOrder")]
    partial class UpdateModelOrder
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Domain.Models.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CustomerEmail")
                        .HasColumnType("text");

                    b.Property<string>("CustomerName")
                        .HasColumnType("text");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("Domain.Models.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<string>("DeliveryAddress")
                        .HasColumnType("text");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("numeric");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("UserId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("Domain.Models.OrderItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("OrderItemPrice")
                        .HasColumnType("numeric");

                    b.Property<int>("OrderItemQuantity")
                        .HasColumnType("integer");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderItems");
                });

            modelBuilder.Entity("Domain.Models.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ProductDescription")
                        .HasColumnType("text");

                    b.Property<string>("ProductName")
                        .HasColumnType("text");

                    b.Property<decimal>("ProductPrice")
                        .HasColumnType("numeric");

                    b.Property<int>("ProductStockQuantity")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Domain.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("FailedLoginAttempts")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UserEmail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserPassword")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Domain.Models.Customer", b =>
                {
                    b.HasOne("Domain.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Models.Order", b =>
                {
                    b.HasOne("Domain.Models.Customer", "Customer")
                        .WithMany("Orders")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Customer");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Domain.Models.OrderItem", b =>
                {
                    b.HasOne("Domain.Models.Order", "Order")
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Domain.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("Domain.Models.Customer", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("Domain.Models.Order", b =>
                {
                    b.Navigation("OrderItems");
                });
#pragma warning restore 612, 618
        }
    }
}
