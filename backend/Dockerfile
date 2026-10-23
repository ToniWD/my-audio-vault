# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# Build stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so the NuGet layer is cached while only source files change.
COPY src/Core/MusicStream.Core.csproj src/Core/
COPY src/Application/MusicStream.Application.csproj src/Application/
COPY src/Infrastructure/MusicStream.Infrastructure.csproj src/Infrastructure/
COPY src/API/MusicStream.API.csproj src/API/
RUN dotnet restore src/API/MusicStream.API.csproj

COPY src/ src/

WORKDIR /src/src/API
RUN dotnet publish MusicStream.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Runtime stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Production by default: appsettings.json + environment variables only.
# Everything in appsettings.json can be overridden with env vars such as
# ConnectionStrings__DefaultConnection (see docker-compose.yml).
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_NOLOGO=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=true \
    DOTNET_RUNNING_IN_CONTAINER=true

# curl is used by the container/compose health check.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# The .NET images ship a non-root user (UID 1654) that can bind to port 8080.
USER $APP_UID

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl --fail --silent http://localhost:8080/api/auth/health || exit 1

ENTRYPOINT ["dotnet", "MusicStream.API.dll"]
