# Stage build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia solo il csproj dell'API postgres
COPY apipostgres/apipostgres.csproj apipostgres/
WORKDIR /src/apipostgres
RUN dotnet restore apipostgres.csproj

# Copia tutto il codice
WORKDIR /src
COPY . .

# Builda e pubblica
WORKDIR /src/apipostgres
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "apipostgres.dll"]
