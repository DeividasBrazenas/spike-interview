FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/Hub/Hub.csproj ./src/Hub/

RUN dotnet restore ./src/Hub/Hub.csproj

COPY src ./src

WORKDIR /src/src/Hub
RUN dotnet build "Hub.csproj" -c $BUILD_CONFIGURATION -o /app/build


# Publish
FROM build AS publish
RUN dotnet publish "Hub.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Spike.Hub.dll"]
