#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Wish.JournalEventSvc/Wish.JournalEventSvc.csproj", "src/Wish.JournalEventSvc/"]
COPY ["src/Wish.JournalApplication/Wish.JournalApplication.csproj", "src/Wish.JournalApplication/"]
COPY ["src/Wish.Services/Wish.Services.csproj", "src/Wish.Services/"]
COPY ["src/Wish/Wish.csproj", "src/Wish/"]
COPY ["src/Wish.JournalAzureTableStorage/Wish.JournalAzureTableStorage.csproj", "src/Wish.JournalAzureTableStorage/"]
RUN dotnet restore "./src/Wish.JournalEventSvc/./Wish.JournalEventSvc.csproj"
COPY . .
WORKDIR "/src/src/Wish.JournalEventSvc"
RUN dotnet build "./Wish.JournalEventSvc.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Wish.JournalEventSvc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wish.JournalEventSvc.dll"]