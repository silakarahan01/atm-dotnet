# Çok aşamalı (multi-stage) derleme — küçük ve güvenli runtime imajı
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Önce proje dosyalarını kopyalayıp restore ederek katman önbelleğinden yararlan
COPY src/ATM.Domain/ATM.Domain.csproj src/ATM.Domain/
COPY src/ATM.Application/ATM.Application.csproj src/ATM.Application/
COPY src/ATM.Infrastructure/ATM.Infrastructure.csproj src/ATM.Infrastructure/
COPY src/ATM.API/ATM.API.csproj src/ATM.API/
RUN dotnet restore src/ATM.API/ATM.API.csproj

# Kaynakların tamamını kopyalayıp yayınla
COPY src/ src/
RUN dotnet publish src/ATM.API/ATM.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ATM.API.dll"]
