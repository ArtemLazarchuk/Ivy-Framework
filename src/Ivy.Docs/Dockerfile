FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5010

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION

RUN test -n "$IVY_PACKAGE_VERSION" || (echo "IVY_PACKAGE_VERSION build-arg is required" && exit 1)

WORKDIR /src

COPY ["src/Ivy.Docs/Ivy.Docs.csproj", "src/Ivy.Docs/"]
COPY ["src/Ivy.Docs.Shared/Ivy.Docs.Shared.csproj", "src/Ivy.Docs.Shared/"]
COPY ["src/Ivy.Docs.Tools/Ivy.Docs.Tools.csproj", "src/Ivy.Docs.Tools/"]

RUN dotnet restore "src/Ivy.Docs/Ivy.Docs.csproj" -p:IvyPackageVersion=$IVY_PACKAGE_VERSION

COPY . .

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG IVY_PACKAGE_VERSION

WORKDIR "/src/src/Ivy.Docs"

RUN dotnet publish "Ivy.Docs.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:IvyPackageVersion=$IVY_PACKAGE_VERSION

FROM base AS final
WORKDIR /app
ENV PORT=5010

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Ivy.Docs.dll"]
