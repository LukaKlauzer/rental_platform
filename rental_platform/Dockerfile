
# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install the certificate generation tools
RUN apt-get update && apt-get install -y openssl
RUN apt-get update && apt-get install -y curl

# Generate self-signed certificate
RUN openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /app/server.key -out /app/server.crt \
    -subj "/CN=localhost"

# Create the PFX file
RUN openssl pkcs12 -export -out /app/server.pfx -inkey /app/server.key -in /app/server.crt -passout pass:Str0ng_Passw0rd!


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["rental_platform/rental_platform.csproj", "rental_platform/"]
RUN dotnet restore "./rental_platform/rental_platform.csproj"
COPY . .
WORKDIR "/src/rental_platform"
RUN dotnet build "./rental_platform.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./rental_platform.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "rental_platform.dll"]