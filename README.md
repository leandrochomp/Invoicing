# InvoicingApi
* C# 13.0
* WebAPI (net 9.0)
* GRPC
* BFF (Backend for Frontend) - TODO

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