#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine AS base

# Instalamos los prerequisitos para la cultura
RUN apk add --no-cache \
        icu-data-full \
        icu-libs

# habilitamos todas las culturas (está seteado en true en la imagen base)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["NodoCliente/NodoCliente.csproj", "NodoCliente/"]
RUN dotnet restore "NodoCliente/NodoCliente.csproj"
COPY . .
WORKDIR "/src/NodoCliente"
RUN dotnet build "NodoCliente.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NodoCliente.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NodoCliente.dll"]