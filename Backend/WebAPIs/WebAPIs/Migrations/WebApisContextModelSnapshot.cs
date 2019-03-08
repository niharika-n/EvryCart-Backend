﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebAPIs.Data;

namespace WebAPIs.Migrations
{
    [DbContext(typeof(WebApisContext))]
    partial class WebApisContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("WebAPIs.Models.AssignedRolesModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("RoleID");

                    b.Property<int>("UserID");

                    b.HasKey("ID");

                    b.ToTable("AssignedRolesTable");
                });

            modelBuilder.Entity("WebAPIs.Models.CategoryModel", b =>
                {
                    b.Property<int>("CategoryID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CategoryDescription")
                        .IsRequired();

                    b.Property<string>("CategoryName")
                        .IsRequired();

                    b.Property<int?>("ChildCategory");

                    b.Property<int>("CreatedBy");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<int?>("ImageID");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<int?>("ModifiedBy");

                    b.Property<DateTime?>("ModifiedDate");

                    b.Property<bool>("ParentCategory");

                    b.HasKey("CategoryID");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("WebAPIs.Models.ContentModel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Content")
                        .IsRequired();

                    b.Property<string>("TemplateName")
                        .IsRequired();

                    b.HasKey("ID");

                    b.ToTable("ContentTable");
                });

            modelBuilder.Entity("WebAPIs.Models.Images", b =>
                {
                    b.Property<int>("ImageID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ImageContent")
                        .IsRequired();

                    b.Property<string>("ImageExtenstion")
                        .IsRequired();

                    b.Property<string>("ImageName")
                        .IsRequired();

                    b.Property<int>("ReferenceID");

                    b.HasKey("ImageID");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("WebAPIs.Models.PasswordResetModel", b =>
                {
                    b.Property<int>("ChangeID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("OldPassword")
                        .IsRequired();

                    b.Property<bool>("PasswordChanged");

                    b.Property<DateTime>("ResetDate");

                    b.Property<string>("Token")
                        .IsRequired();

                    b.Property<DateTime>("TokenTimeOut");

                    b.Property<int>("UserID");

                    b.HasKey("ChangeID");

                    b.ToTable("PasswordResetTable");
                });

            modelBuilder.Entity("WebAPIs.Models.ProductAttribute", b =>
                {
                    b.Property<int>("AttributeID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AttributeName")
                        .IsRequired();

                    b.Property<int>("CreatedBy");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<int?>("ModifiedBy");

                    b.Property<DateTime?>("ModifiedDate");

                    b.HasKey("AttributeID");

                    b.ToTable("ProductAttributes");
                });

            modelBuilder.Entity("WebAPIs.Models.ProductAttributeValues", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AttributeID");

                    b.Property<int>("ProductID");

                    b.Property<string>("Value")
                        .IsRequired();

                    b.HasKey("ID");

                    b.HasIndex("AttributeID");

                    b.HasIndex("ProductID");

                    b.ToTable("ProductAttributeValues");
                });

            modelBuilder.Entity("WebAPIs.Models.ProductImage", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ImageContent")
                        .IsRequired();

                    b.Property<string>("ImageExtenstion")
                        .IsRequired();

                    b.Property<string>("ImageName")
                        .IsRequired();

                    b.Property<int>("ProductID");

                    b.HasKey("ID");

                    b.HasIndex("ProductID");

                    b.ToTable("ProductImage");
                });

            modelBuilder.Entity("WebAPIs.Models.ProductModel", b =>
                {
                    b.Property<int>("ProductID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("AllowCustomerReviews");

                    b.Property<int>("CategoryID");

                    b.Property<int>("CreatedBy");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<int?>("DiscountPercent");

                    b.Property<bool>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<bool>("IsDiscounted");

                    b.Property<string>("LongDescription")
                        .IsRequired();

                    b.Property<bool>("MarkNew");

                    b.Property<string>("ModelNumber")
                        .IsRequired();

                    b.Property<int?>("ModifiedBy");

                    b.Property<DateTime?>("ModifiedDate");

                    b.Property<bool>("OnHomePage");

                    b.Property<int>("Price");

                    b.Property<string>("ProductName")
                        .IsRequired();

                    b.Property<int>("QuantityInStock");

                    b.Property<string>("QuantityType")
                        .IsRequired();

                    b.Property<bool>("ShipingEnabled");

                    b.Property<int?>("ShippingCharges");

                    b.Property<string>("ShortDescription")
                        .IsRequired();

                    b.Property<int?>("Tax");

                    b.Property<bool>("TaxExempted");

                    b.Property<DateTime>("VisibleEndDate");

                    b.Property<DateTime>("VisibleStartDate");

                    b.HasKey("ProductID");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("WebAPIs.Models.UserModel", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("EmailID");

                    b.Property<string>("FirstName");

                    b.Property<string>("ImageContent");

                    b.Property<string>("LastName");

                    b.Property<string>("Password");

                    b.Property<string>("Username");

                    b.HasKey("UserID");

                    b.ToTable("Login");
                });

            modelBuilder.Entity("WebAPIs.Models.UserRoleModel", b =>
                {
                    b.Property<int>("RoleID")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("RoleDescription")
                        .IsRequired();

                    b.Property<string>("RoleName")
                        .IsRequired();

                    b.HasKey("RoleID");

                    b.ToTable("UserRoleTable");
                });

            modelBuilder.Entity("WebAPIs.Models.ProductAttributeValues", b =>
                {
                    b.HasOne("WebAPIs.Models.ProductAttribute", "Attribute")
                        .WithMany()
                        .HasForeignKey("AttributeID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WebAPIs.Models.ProductModel", "Product")
                        .WithMany()
                        .HasForeignKey("ProductID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WebAPIs.Models.ProductImage", b =>
                {
                    b.HasOne("WebAPIs.Models.ProductModel", "Product")
                        .WithMany()
                        .HasForeignKey("ProductID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
