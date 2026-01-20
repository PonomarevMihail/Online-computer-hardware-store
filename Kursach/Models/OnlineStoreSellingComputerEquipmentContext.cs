using KursachBD.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;

namespace KursachBD.Models;

public partial class OnlineStoreSellingComputerEquipmentContext : DbContext
{
    public OnlineStoreSellingComputerEquipmentContext()
    {
    }

    public OnlineStoreSellingComputerEquipmentContext(DbContextOptions<OnlineStoreSellingComputerEquipmentContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ContentOfOrder> ContentOfOrders { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<DeliveryNoteContent> DeliveryNoteContents { get; set; }

    public virtual DbSet<DeliveryNoteHead> DeliveryNoteHeads { get; set; }

    public virtual DbSet<LegalEntity> LegalEntities { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductPriceHistory> ProductPriceHistories { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=localhost;Database=online_store_selling_computer_equipment;Username=Mikhail;Password=1_Mi7h9_1;Search Path=store;Persist Security Info=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("store");
        modelBuilder.HasPostgresEnum("store", "product_type_enum", new[] { "laptop", "desktop", "all_in_one", "workstation", "server", "processor", "motherboard", "ram", "video_card", "power_supply", "computer_case", "cooling_system", "ssd", "hdd", "external_drive", "flash_drive", "memory_card", "monitor", "keyboard", "mouse", "headphones", "webcam", "microphone", "speakers", "router", "network_adapter", "wifi_extender", "printer", "scanner", "multifunction_device", "mouse_pad", "usb_hub", "cable", "adapter", "laptop_bag", "gaming_mouse", "gaming_keyboard", "gamepad", "joystick", "vr_headset", "software", "component", "accessory" });

        modelBuilder.Entity<ContentOfOrder>(entity =>
        {
            entity.HasKey(e => new { e.NumberOfOrder, e.ProductId }).HasName("content_of_order_pkey");

            entity.ToTable("content_of_order", "store");

            entity.HasIndex(e => new { e.NumberOfOrder, e.ProductId }, "idx_content_order_product");

            entity.Property(e => e.NumberOfOrder).HasColumnName("number_of_order");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.NumberOfOrderNavigation).WithMany(p => p.ContentOfOrders)
                .HasForeignKey(d => d.NumberOfOrder)
                .HasConstraintName("content_of_order_number_of_order_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.ContentOfOrders)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("content_of_order_product_id_fkey");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customer_pkey");

            entity.ToTable("customer", "store");

            entity.HasIndex(e => e.Email, "customer_email_key").IsUnique();

            entity.HasIndex(e => e.Login, "customer_login_key").IsUnique();

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(72)
                .HasColumnName("password");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(100)
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasMaxLength(12)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Surname)
                .HasMaxLength(100)
                .HasColumnName("surname");

            entity.HasOne(d => d.Role).WithMany(p => p.Customers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("customer_role_id_fkey");


            modelBuilder.HasPostgresEnum<ProductType>("product_type_enum");
        });

        modelBuilder.Entity<DeliveryNoteContent>(entity =>
        {
            entity.HasKey(e => e.IdNc).HasName("delivery_note_content_pkey");

            entity.ToTable("delivery_note_content", "store");

            entity.HasIndex(e => new { e.IdNh, e.ProductId }, "idx_delivery_note_head");

            entity.Property(e => e.IdNc).HasColumnName("id_nc");
            entity.Property(e => e.IdNh).HasColumnName("id_nh");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.IdNhNavigation).WithMany(p => p.DeliveryNoteContents)
                .HasForeignKey(d => d.IdNh)
                .HasConstraintName("delivery_note_content_id_nh_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.DeliveryNoteContents)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("delivery_note_content_product_id_fkey");
        });

        modelBuilder.Entity<DeliveryNoteHead>(entity =>
        {
            entity.HasKey(e => e.IdNh).HasName("delivery_note_head_pkey");

            entity.ToTable("delivery_note_head", "store");

            entity.Property(e => e.IdNh).HasColumnName("id_nh");
            entity.Property(e => e.Buyer)
                .HasMaxLength(150)
                .HasColumnName("buyer");
            entity.Property(e => e.Consignee)
                .HasMaxLength(150)
                .HasColumnName("consignee");
            entity.Property(e => e.DeliveryDate).HasColumnName("delivery_date");
            entity.Property(e => e.Salesman)
                .HasMaxLength(150)
                .HasColumnName("salesman");
            entity.Property(e => e.Shipper)
                .HasMaxLength(150)
                .HasColumnName("shipper");
            entity.Property(e => e.SummaryPrice)
                .HasPrecision(12, 2)
                .HasColumnName("summary_price");

            entity.HasOne(d => d.BuyerNavigation).WithMany(p => p.DeliveryNoteHeadBuyerNavigations)
                .HasForeignKey(d => d.Buyer)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("delivery_note_head_buyer_fkey");

            entity.HasOne(d => d.ConsigneeNavigation).WithMany(p => p.DeliveryNoteHeadConsigneeNavigations)
                .HasForeignKey(d => d.Consignee)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("delivery_note_head_consignee_fkey");

            entity.HasOne(d => d.SalesmanNavigation).WithMany(p => p.DeliveryNoteHeadSalesmanNavigations)
                .HasForeignKey(d => d.Salesman)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("delivery_note_head_salesman_fkey");

            entity.HasOne(d => d.ShipperNavigation).WithMany(p => p.DeliveryNoteHeadShipperNavigations)
                .HasForeignKey(d => d.Shipper)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("delivery_note_head_shipper_fkey");
        });

        modelBuilder.Entity<LegalEntity>(entity =>
        {
            entity.HasKey(e => e.Inn).HasName("legal_entity_pkey");

            entity.ToTable("legal_entity", "store");

            entity.HasIndex(e => e.Company, "idx_legal_entity_company");

            entity.Property(e => e.Inn)
                .HasMaxLength(12)
                .HasColumnName("inn");
            entity.Property(e => e.Company)
                .HasMaxLength(255)
                .HasColumnName("company");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(100)
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasMaxLength(12)
                .HasColumnName("phone");
            entity.Property(e => e.Surname)
                .HasMaxLength(100)
                .HasColumnName("surname");
        });

        modelBuilder.Entity<Order>().ToTable("\"Order\"");
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.NumberOfOrder).HasName("Order_pkey");

            entity.ToTable("Order", "store");

            entity.Property(e => e.NumberOfOrder).HasColumnName("number_of_order");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DeliveryAddress).HasColumnName("delivery_address");
            entity.Property(e => e.DeliveryDate).HasColumnName("delivery_date");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
        });


        modelBuilder.Entity<Product>().ToTable("product", "store");
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("product_pkey");

            entity.ToTable("product", "store");

            entity.HasIndex(e => new { e.Price, e.Manufacturer }, "idx_product_price_manufacturer");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Characteristics)
                .HasColumnType("jsonb")
                .HasColumnName("characteristics");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(255)
                .HasColumnName("manufacturer");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(12, 2)
                .HasColumnName("price");
            entity.Property(e => e.Stock)
                .HasDefaultValue(0L)
                .HasColumnName("stock");
            entity.Property(e => e.Weight)
                .HasPrecision(5, 3)
                .HasColumnName("weight");
        });

        modelBuilder.Entity<ProductPriceHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("product_price_history_pkey");

            entity.ToTable("product_price_history", "store");

            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy)
                .HasDefaultValueSql("CURRENT_USER")
                .HasColumnName("changed_by");
            entity.Property(e => e.NewPrice)
                .HasPrecision(12, 2)
                .HasColumnName("new_price");
            entity.Property(e => e.OldPrice)
                .HasPrecision(12, 2)
                .HasColumnName("old_price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductPriceHistories)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_price_history_product_id_fkey");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.NumberOfReview).HasName("reviews_pkey");

            entity.ToTable("reviews", "store");

            entity.HasIndex(e => new { e.CustomerId, e.ProductId }, "idx_reviews_customer_product");

            entity.HasIndex(e => new { e.ProductId, e.CreatedAt }, "idx_reviews_product_created");

            entity.HasIndex(e => new { e.CustomerId, e.ProductId }, "reviews_customer_id_product_id_key").IsUnique();

            entity.Property(e => e.NumberOfReview).HasColumnName("number_of_review");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Review1).HasColumnName("review");

            entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("reviews_product_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("role_pkey");

            entity.ToTable("role", "store");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(100)
                .HasColumnName("job_title");
        });

        modelBuilder.Entity<ProductPriceHistory>().ToTable("product_price_history", schema: "store");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
