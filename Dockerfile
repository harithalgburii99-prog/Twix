# ── Stage 1: Build ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore (cached layer)
COPY Twix.csproj .
RUN dotnet restore

# Copy source and publish
COPY . .
RUN dotnet publish Twix.csproj -c Release -o /app/publish

# ── Stage 2: Runtime ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Persistent SQLite volume directory
RUN mkdir -p /data

# Copy published app (includes wwwroot)
COPY --from=build /app/publish .

# Verify wwwroot was included (debug — remove if desired)
RUN ls -la wwwroot/ && ls -la wwwroot/css/

ENV PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DB_PATH=/data/twix.db

EXPOSE 8080

ENTRYPOINT ["dotnet", "Twix.dll"]
