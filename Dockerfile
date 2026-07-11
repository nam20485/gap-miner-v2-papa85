# Dockerfile — multi-stage build for the GapMiner .NET 8 solution.
#
# Builds the entire solution, then publishes the project selected by the
# PROJECT build argument (defaults to the Api gateway). Each app service in
# docker-compose.yml reuses this file with a different PROJECT value:
#
#   docker build -t gapminer-api   --build-arg PROJECT=GapMiner.Api .
#   docker build -t gapminer-web   --build-arg PROJECT=GapMiner.Web .
#   docker build -t gapminer-scrape --build-arg PROJECT=GapMiner.ScraperWorker .
#   docker build -t gapminer-ai    --build-arg PROJECT=GapMiner.AIWorker .
#
# Syntax: docker/dockerfile:1.7 for heredoc + BuildKit features.
#syntax=docker/dockerfile:1.7

# ----------------------------------------------------------------------------
# Stage 1 — build
# ----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG PROJECT=GapMiner.Api
ENV PROJECT=${PROJECT}

WORKDIR /src

# Copy central package management + build props first to maximize layer caching.
COPY Directory.Packages.props Directory.Build.props ./
COPY GapMiner.sln ./

# Copy all project files (enables deterministic restore before source copy).
COPY src/ ./src/
COPY tests/ ./tests/

# Restore the solution.
RUN dotnet restore GapMiner.sln

# Build and publish the selected project.
RUN dotnet publish "src/${PROJECT}/${PROJECT}.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ----------------------------------------------------------------------------
# Stage 2 — runtime
# ----------------------------------------------------------------------------
# Api/Web use ASP.NET Core; workers only need the base runtime. Use the
# ASP.NET Core image for broad compatibility across all services.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ARG PROJECT=GapMiner.Api
ENV PROJECT=${PROJECT}

WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "${PROJECT}.dll"]
