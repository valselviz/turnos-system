# Etapa 1: compilar y publicar con el SDK completo de .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Turnos.Api.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Etapa 2: imagen final, solo con el runtime de ASP.NET (mucho más liviana,
# no tiene el compilador ni las herramientas de desarrollo)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Turnos.Api.dll"]
