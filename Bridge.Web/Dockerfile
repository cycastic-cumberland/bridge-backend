FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
VOLUME ["/app/appsettings.json"]

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bridge.Web/Bridge.Web.csproj", "Bridge.Web/"]
COPY ["Bridge.Core/Bridge.Core.csproj", "Bridge.Core/"]
COPY ["Bridge.Infrastructure.Abstractions/Bridge.Infrastructure.Abstractions.csproj", "Bridge.Infrastructure.Abstractions/"]
COPY ["Bridge.Domain/Bridge.Domain.csproj", "Bridge.Domain/"]
COPY ["Bridge.Infrastructure.Data.Postgres/Bridge.Infrastructure.Data.Postgres.csproj", "Bridge.Infrastructure.Data.Postgres/"]
COPY ["Bridge.Infrastructure.Data/Bridge.Infrastructure.Data.csproj", "Bridge.Infrastructure.Data/"]
RUN dotnet restore "Bridge.Web/Bridge.Web.csproj"
COPY . .
WORKDIR "/src/Bridge.Web"
RUN dotnet build "Bridge.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Bridge.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bridge.Web.dll"]
