FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY DVLD.slnx .
COPY DVLD.API/DVLD.API.csproj             DVLD.API/
COPY DVLD.CORE/DVLD.CORE.csproj           DVLD.CORE/
COPY DVLD.SERVICES/DVLD.SERVICES.csproj   DVLD.SERVICES/
COPY DVLD.INFRASTRUCTURE/DVLD.INFRASTRUCTURE.csproj  DVLD.INFRASTRUCTURE/
COPY DVLD.Tests/DVLD.Tests.csproj         DVLD.Tests/

RUN dotnet restore DVLD.slnx

COPY . .

RUN dotnet publish DVLD.API/DVLD.API.csproj \
    -c Release \
    -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p /app/uploads

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DVLD.API.dll"]