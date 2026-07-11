# ---------------------------------------------------------
# Runtime stage
# ---------------------------------------------------------

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER app
WORKDIR /app

EXPOSE 8080

# ---------------------------------------------------------
# Build stage
# ---------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy project files separately for Docker layer caching.
COPY ["src/Enhanzer.Assignment.Api/Enhanzer.Assignment.Api.csproj", "src/Enhanzer.Assignment.Api/"]
COPY ["src/Enhanzer.Assignment.Application/Enhanzer.Assignment.Application.csproj", "src/Enhanzer.Assignment.Application/"]
COPY ["src/Enhanzer.Assignment.Domain/Enhanzer.Assignment.Domain.csproj", "src/Enhanzer.Assignment.Domain/"]
COPY ["src/Enhanzer.Assignment.Infrastructure/Enhanzer.Assignment.Infrastructure.csproj", "src/Enhanzer.Assignment.Infrastructure/"]

RUN dotnet restore \
    "src/Enhanzer.Assignment.Api/Enhanzer.Assignment.Api.csproj"

# Copy all source files.
COPY . .

WORKDIR "/src/src/Enhanzer.Assignment.Api"

RUN dotnet build \
    "Enhanzer.Assignment.Api.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/build \
    --no-restore

# ---------------------------------------------------------
# Publish stage
# ---------------------------------------------------------

FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish \
    "Enhanzer.Assignment.Api.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# ---------------------------------------------------------
# Final stage
# ---------------------------------------------------------

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} exec dotnet Enhanzer.Assignment.Api.dll"]