FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

EXPOSE 10000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["GiftOfTheGivers.Helpers/GiftOfTheGivers.Helpers.csproj", "GiftOfTheGivers.Helpers/"]
COPY ["GiftOfTheGivers.Web/GiftOfTheGivers.Web.csproj", "GiftOfTheGivers.Web/"]

RUN dotnet restore "GiftOfTheGivers.Web/GiftOfTheGivers.Web.csproj"

COPY . .

WORKDIR /src/GiftOfTheGivers.Web
RUN dotnet publish "GiftOfTheGivers.Web.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} exec dotnet GiftOfTheGivers.Web.dll"]
