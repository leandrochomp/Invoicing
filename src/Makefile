.PHONY: build up down ps logs api grpc bff migration migration-add migration-remove migration-script clean database-check

# Default Docker Compose project name
COMPOSE_PROJECT_NAME ?= invoicing

# Default values
ENV ?= Development
MIGRATION_NAME ?= InitialCreate

# Docker Compose command prefix
DOCKER_COMPOSE := docker compose

# Main targets
build:
	$(DOCKER_COMPOSE) build

up:
	$(DOCKER_COMPOSE) up -d

down:
	$(DOCKER_COMPOSE) down

ps:
	$(DOCKER_COMPOSE) ps

logs:
	$(DOCKER_COMPOSE) logs -f

# Service targets
api:
	$(DOCKER_COMPOSE) up -d api

grpc:
	$(DOCKER_COMPOSE) up -d grpc

bff:
	$(DOCKER_COMPOSE) up -d bff

# Database migration targets
migration:
	$(DOCKER_COMPOSE) up migrations

migration-add:
	$(DOCKER_COMPOSE) run --rm migrations bash -c "dotnet ef migrations add $(MIGRATION_NAME) --project Shared/Shared.csproj --context AppDbContext --verbose"

migration-remove:
	$(DOCKER_COMPOSE) run --rm migrations bash -c "dotnet ef migrations remove --project Shared/Shared.csproj --context AppDbContext"

migration-script:
	$(DOCKER_COMPOSE) run --rm migrations bash -c "dotnet ef migrations script --project Shared/Shared.csproj --context AppDbContext --output /src/Migrations/script.sql"

database-check:
	$(DOCKER_COMPOSE) exec postgres psql -U postgres -d invoicing -c '\dt'

# Clean everything
clean:
	$(DOCKER_COMPOSE) down -v --remove-orphans
	rm -rf **/bin/ **/obj/

