# ==============================
# BUILD
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["Eticaret.Core/Eticaret.Core.csproj", "Eticaret.Core/"]
COPY ["Eticaret.Data/Eticaret.Data.csproj", "Eticaret.Data/"]
COPY ["Eticaret.Service/Eticaret.Service.csproj", "Eticaret.Service/"]
COPY ["Eticaret.WebUI/Eticaret.WebUI.csproj", "Eticaret.WebUI/"]

RUN dotnet restore "Eticaret.WebUI/Eticaret.WebUI.csproj"

COPY . .

WORKDIR "/src/Eticaret.WebUI"

RUN dotnet publish "Eticaret.WebUI.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ==============================
# RUNTIME
# ==============================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "Eticaret.WebUI.dll"]