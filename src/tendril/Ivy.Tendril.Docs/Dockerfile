FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION

WORKDIR /src
ARG IVY_PACKAGE_VERSION

# Copy projects for restoration
COPY ["src/tendril/Ivy.Tendril.Docs/Ivy.Tendril.Docs.csproj", "src/tendril/Ivy.Tendril.Docs/"]
COPY ["src/Ivy/Ivy.csproj", "src/Ivy/"]
COPY ["src/Ivy.Docs.Helpers/Ivy.Docs.Helpers.csproj", "src/Ivy.Docs.Helpers/"]
COPY ["src/Ivy.Analyser/Ivy.Analyser.csproj", "src/Ivy.Analyser/"]
COPY ["src/Ivy.Agent.Filter/Ivy.Agent.Filter.csproj", "src/Ivy.Agent.Filter/"]

# Restore
RUN dotnet restore "src/tendril/Ivy.Tendril.Docs/Ivy.Tendril.Docs.csproj" -p:IvyPackageVersion=$IVY_PACKAGE_VERSION

# Copy the rest of the source code
COPY . .

# Build and publish
WORKDIR "/src/src/tendril/Ivy.Tendril.Docs"
RUN dotnet publish "Ivy.Tendril.Docs.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:IvyPackageVersion=$IVY_PACKAGE_VERSION

FROM base AS final
WORKDIR /app
USER root
RUN apt-get update && apt-get install -y --no-install-recommends \
    libc6 \
    libgcc-s1 \
    && apt-get clean && rm -rf /var/lib/apt/lists/*
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ivy.Tendril.Docs.dll"]
