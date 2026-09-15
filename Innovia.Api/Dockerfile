FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Innovia.Api/Innovia.Api.csproj Innovia.Api/
RUN dotnet restore Innovia.Api/Innovia.Api.csproj

COPY Innovia.Api/ Innovia.Api/
WORKDIR /src/Innovia.Api
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app .
USER app

ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Innovia.Api.dll"]