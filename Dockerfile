# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Assistant.sln .
COPY src/Assistant.API/Assistant.API.csproj src/Assistant.API/
COPY src/Assistant.Core/Assistant.Core.csproj src/Assistant.Core/
COPY src/Assistant.Infrastructure/Assistant.Infrastructure.csproj src/Assistant.Infrastructure/
COPY src/Assistant.Shared/Assistant.Shared.csproj src/Assistant.Shared/

# Restore dependencies
RUN dotnet restore Assistant.sln

# Copy everything else
COPY . .

# Build
WORKDIR /src/src/Assistant.API
RUN dotnet build "Assistant.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Assistant.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Assistant.API.dll"]
