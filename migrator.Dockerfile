FROM mcr.microsoft.com/dotnet/sdk:10.0 AS migrator
WORKDIR /src

COPY src/EnvikServer.Core/EnvikServer.Core.csproj src/EnvikServer.Core/
COPY src/EnvikServer.Application/EnvikServer.Application.csproj src/EnvikServer.Application/
COPY src/EnvikServer.Infrastructure/EnvikServer.Infrastructure.csproj src/EnvikServer.Infrastructure/
COPY src/EnvikServer.API/EnvikServer.API.csproj src/EnvikServer.API/
RUN dotnet restore src/EnvikServer.API/EnvikServer.API.csproj

COPY src/ src/
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

ENTRYPOINT ["dotnet-ef", "database", "update", "--project", "src/EnvikServer.Infrastructure", "--startup-project", "src/EnvikServer.API"]