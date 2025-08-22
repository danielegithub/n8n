# Stage build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia solo il csproj dell'API
COPY api/api.csproj api/
WORKDIR /src/api
RUN dotnet restore api.csproj

# Copia tutto il codice
WORKDIR /src
COPY . .

# Builda e pubblica
WORKDIR /src/api
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "api.dll"]
