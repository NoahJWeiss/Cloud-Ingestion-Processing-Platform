FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY CloudIngestion.sln .
COPY src/CloudIngestion.Core/CloudIngestion.Core.csproj src/CloudIngestion.Core/
COPY src/CloudIngestion.Infrastructure/CloudIngestion.Infrastructure.csproj src/CloudIngestion.Infrastructure/
COPY src/CloudIngestion.Worker/CloudIngestion.Worker.csproj src/CloudIngestion.Worker/

RUN dotnet restore src/CloudIngestion.Worker/CloudIngestion.Worker.csproj

COPY src/CloudIngestion.Core/ src/CloudIngestion.Core/
COPY src/CloudIngestion.Infrastructure/ src/CloudIngestion.Infrastructure/
COPY src/CloudIngestion.Worker/ src/CloudIngestion.Worker/

RUN dotnet publish src/CloudIngestion.Worker/CloudIngestion.Worker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/uploads
ENTRYPOINT ["dotnet", "CloudIngestion.Worker.dll"]
