# InvoicingApi
* C# 13.0
* WebAPI (net 9.0)
* GRPC
* BFF (Backend for Frontend) - TODO
* EF Core

# Development Tools:
* dotnet cli
* Jetbrains Rider 2025.1.2 (Non-commercial)
* Ubuntu 24.04.2 LTS

# CLI Commands:
`dotnet new webapi -o InvoicingApi`
* root folder:

```bash
dotnet new sln -n Invoicing.sln
dotnet sln Invoicing.sln add src/App/InvoicingApi/InvoicingApi.csproj
```

```bash
dotnet new web -o src/App/InvoicingBff
dotnet new grpc -o src/App/InvoicingGrpc
dotnet sln Invoicing.sln add src/App/InvoicingBff/InvoicingBff.csproj src/App/InvoicingGrpc/InvoicingGrpc.csproj
```

# Database Migrations

## Prerequisites
- PostgreSQL installed and running
- Connection string properly configured in appsettings.json

## Running Migrations

### Create a new migration
in the root folder of the solution, run the following command to create a new migration:
```bash
dotnet ef migrations add InitialCreate --project Shared --startup-project App/InvoicingApi
```

### Generate SQL script(optional)
in the root folder of the solution, run the following command to generate a SQL script for the migration:
```bash
dotnet ef migrations script --project Shared --startup-project App/InvoicingApi
```

### Apply migrations
in the root folder of the solution, run the following command to apply the migrations to the database:
```bash
dotnet ef database update --project Shared --startup-project App/InvoicingApi
```

### Rollback a migration
in the root folder of the solution, run the following command to rollback the last migration:
```bash
dotnet ef database remove --project Shared --startup-project App/InvoicingApi
```