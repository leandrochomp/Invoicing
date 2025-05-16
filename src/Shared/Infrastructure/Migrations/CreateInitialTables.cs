using FluentMigrator;

namespace Shared.Infrastructure.Migrations;

[Migration(202505160001)]
public class CreateInitialTables : Migration
{
    public override void Up()
    {
        Create.Table("clients")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("email").AsString(200).NotNullable()
            .WithColumn("company_name").AsString(200).Nullable()
            .WithColumn("address").AsString(500).Nullable()
            .WithColumn("phone_number").AsString(20).Nullable()
            .WithColumn("created_at").AsCustom("timestamp with time zone").NotNullable()
            .WithColumn("updated_at").AsCustom("timestamp with time zone").Nullable()
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.Table("invoices")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("client_id").AsGuid().NotNullable()
            .WithColumn("invoice_number").AsString(50).NotNullable()
            .WithColumn("issue_date").AsCustom("timestamp with time zone").NotNullable()
            .WithColumn("due_date").AsCustom("timestamp with time zone").NotNullable()
            .WithColumn("status").AsString(20).NotNullable()
            .WithColumn("total_amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("currency").AsString(3).NotNullable()
            .WithColumn("notes").AsString(1000).Nullable()
            .WithColumn("created_at").AsCustom("timestamp with time zone").NotNullable()
            .WithColumn("updated_at").AsCustom("timestamp with time zone").Nullable()
            .WithColumn("is_deleted").AsBoolean().NotNullable().WithDefaultValue(false);

        Create.ForeignKey("invoice_clientId_fk")
            .FromTable("invoices").ForeignColumn("client_id")
            .ToTable("clients").PrimaryColumn("id");

        Create.Table("invoice_items")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("invoice_id").AsGuid().NotNullable()
            .WithColumn("description").AsString(500).NotNullable()
            .WithColumn("quantity").AsInt32().NotNullable()
            .WithColumn("unit_price").AsDecimal(18, 2).NotNullable()
            .WithColumn("tax_rate").AsDecimal(5, 2).NotNullable()
            .WithColumn("total_amount").AsDecimal(18, 2).NotNullable()
            .WithColumn("sort_order").AsInt32().NotNullable();

        Create.ForeignKey("invoice_item_invoice_id_fk")
            .FromTable("invoice_items").ForeignColumn("invoice_id")
            .ToTable("invoices").PrimaryColumn("id");

        Create.Table("payments")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("invoice_id").AsGuid().NotNullable()
            .WithColumn("amount_paid").AsDecimal(18, 2).NotNullable()
            .WithColumn("payment_date").AsCustom("timestamp with time zone").NotNullable()
            .WithColumn("payment_method").AsString(100).NotNullable()
            .WithColumn("reference_number").AsString(100).Nullable()
            .WithColumn("notes").AsString(1000).Nullable();

        Create.ForeignKey("payment_invoice_id_fk")
            .FromTable("payments").ForeignColumn("invoice_id")
            .ToTable("invoices").PrimaryColumn("id");
    }

    public override void Down()
    {
        Delete.ForeignKey("invoice_item_invoice_id_fk").OnTable("invoice_items");
        Delete.ForeignKey("payment_invoice_id_fk").OnTable("payments");
        Delete.ForeignKey("invoice_clientId_fk").OnTable("invoices");
        Delete.Table("invoice_items");
        Delete.Table("payments");
        Delete.Table("invoices");
        Delete.Table("clients");
    }
}