# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY src/Cure.Domain/Cure.Domain.csproj src/Cure.Domain/
COPY src/Cure.Application/Cure.Application.csproj src/Cure.Application/
COPY src/Cure.Infrastructure/Cure.Infrastructure.csproj src/Cure.Infrastructure/
COPY src/Cure.Api/Cure.Api.csproj src/Cure.Api/

RUN dotnet restore src/Cure.Api/Cure.Api.csproj

# Copy everything and publish
COPY . .
RUN dotnet publish src/Cure.Api -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cure.Api.dll"]
