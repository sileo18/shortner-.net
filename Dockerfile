# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

# Copia arquivos do projeto
COPY *.csproj ./
RUN dotnet restore

# Copia todo o código
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copia o build da etapa anterior
COPY --from=build /app/out .

# Configura portas
EXPOSE 5000
EXPOSE 5001

# Define ambiente de produção
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Comando de inicialização
ENTRYPOINT ["dotnet", "UrlShortner.dll"]
