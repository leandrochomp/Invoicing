FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Environment variables for better NuGet package handling
ENV NUGET_PACKAGES=/nuget/packages
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Copy csproj and restore as distinct layers
COPY ["Invoicing.sln", "./"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["App/InvoicingApi/InvoicingApi.csproj", "App/InvoicingApi/"]
COPY ["InvoicingGrpc/InvoicingGrpc.csproj", "InvoicingGrpc/"]
COPY ["InvoicingBff/InvoicingBff.csproj", "InvoicingBff/"]
COPY ["Shared.UnitTests/Shared.UnitTests.csproj", "Shared.UnitTests/"]

# Copy Directory.* files for centralized package management
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

# First restore to populate the NuGet cache
RUN dotnet restore "Invoicing.sln" #--verbosity normal (debugging)

# Copy everything else 
COPY . .

# Build the solution
RUN dotnet build "Invoicing.sln" -c Release

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef --version 9.0.5

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

FROM build AS migrations
WORKDIR /src
# We'll use this stage for running migrations

FROM build AS publish
# Publish the APIs
RUN dotnet publish "App/InvoicingApi/InvoicingApi.csproj" -c Release -o /app/InvoicingApi
RUN dotnet publish "InvoicingGrpc/InvoicingGrpc.csproj" -c Release -o /app/InvoicingGrpc
RUN dotnet publish "InvoicingBff/InvoicingBff.csproj" -c Release -o /app/InvoicingBff

# Create a base runtime image with common database configuration
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base-runtime
WORKDIR /app

# Accept build arguments for database configuration - defined only once
ARG DB_HOST=postgres
ARG DB_PORT=5432
ARG DB_NAME=invoicing
ARG DB_USER=postgres
ARG DB_PASSWORD=postgres

# Set environment variables from build arguments - defined only once
ENV DB_HOST=${DB_HOST}
ENV DB_PORT=${DB_PORT}
ENV DB_NAME=${DB_NAME}
ENV DB_USER=${DB_USER}
ENV DB_PASSWORD=${DB_PASSWORD}

# Build runtime images for each service - inheriting environment variables from base-runtime
FROM base-runtime AS api
WORKDIR /app
COPY --from=publish /app/InvoicingApi ./
ENTRYPOINT ["dotnet", "InvoicingApi.dll"]

FROM base-runtime AS grpc
WORKDIR /app
COPY --from=publish /app/InvoicingGrpc ./
ENTRYPOINT ["dotnet", "InvoicingGrpc.dll"]

FROM base-runtime AS bff
WORKDIR /app
COPY --from=publish /app/InvoicingBff ./
ENTRYPOINT ["dotnet", "InvoicingBff.dll"]
