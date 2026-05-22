FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY CloudIngestion.sln .
COPY src/CloudIngestion.Core/CloudIngestion.Core.csproj src/CloudIngestion.Core/
COPY src/CloudIngestion.Infrastructure/CloudIngestion.Infrastructure.csproj src/CloudIngestion.Infrastructure/
COPY src/CloudIngestion.Api/CloudIngestion.Api.csproj src/CloudIngestion.Api/

RUN dotnet restore src/CloudIngestion.Api/CloudIngestion.Api.csproj

COPY src/CloudIngestion.Core/ src/CloudIngestion.Core/
COPY src/CloudIngestion.Infrastructure/ src/CloudIngestion.Infrastructure/
COPY src/CloudIngestion.Api/ src/CloudIngestion.Api/

RUN dotnet publish src/CloudIngestion.Api/CloudIngestion.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/uploads
EXPOSE 8080
ENTRYPOINT ["dotnet", "CloudIngestion.Api.dll"]
