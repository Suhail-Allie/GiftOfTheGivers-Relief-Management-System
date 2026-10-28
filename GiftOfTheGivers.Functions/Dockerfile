FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["GiftOfTheGivers.Functions/GiftOfTheGivers.Functions.csproj", "GiftOfTheGivers.Functions/"]

RUN dotnet restore "GiftOfTheGivers.Functions/GiftOfTheGivers.Functions.csproj"

COPY ["GiftOfTheGivers.Functions/", "GiftOfTheGivers.Functions/"]

WORKDIR /src/GiftOfTheGivers.Functions

RUN dotnet publish "GiftOfTheGivers.Functions.csproj" \
    --configuration Release \
    --output /home/site/wwwroot \
    /p:UseAppHost=false

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    FUNCTIONS_WORKER_RUNTIME=dotnet-isolated \
    AzureWebJobsSecretStorageType=files

WORKDIR /home/site/wwwroot

COPY --from=build /home/site/wwwroot/ /home/site/wwwroot/

EXPOSE 80
