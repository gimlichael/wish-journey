FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Wish.JournalApi/Wish.JournalApi.csproj", "src/Wish.JournalApi/"]
COPY ["src/Wish.IpGeolocationService/Wish.IpGeolocationService.csproj", "src/Wish.IpGeolocationService/"]
COPY ["src/Wish.Services/Wish.Services.csproj", "src/Wish.Services/"]
COPY ["src/Wish/Wish.csproj", "src/Wish/"]
COPY ["src/Wish.JournalApplication/Wish.JournalApplication.csproj", "src/Wish.JournalApplication/"]
COPY ["src/Wish.JournalAzureSqlServer/Wish.JournalAzureSqlServer.csproj", "src/Wish.JournalAzureSqlServer/"]
COPY ["src/Wish.JournalAzureTableStorage/Wish.JournalAzureTableStorage.csproj", "src/Wish.JournalAzureTableStorage/"]
COPY ["src/Wish.LocationGeocodeService/Wish.LocationGeocodeService.csproj", "src/Wish.LocationGeocodeService/"]
COPY ["src/Wish.Shared/Wish.Shared.csproj", "src/Wish.Shared/"]
COPY ["src/Wish.WeatherTomorrowService/Wish.WeatherTomorrowService.csproj", "src/Wish.WeatherTomorrowService/"]
RUN dotnet restore "./src/Wish.JournalApi/./Wish.JournalApi.csproj"
COPY . .
WORKDIR "/src/src/Wish.JournalApi"
RUN dotnet build "./Wish.JournalApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Wish.JournalApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wish.JournalApi.dll"]
