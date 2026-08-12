# syntax=docker/dockerfile:1

# ---- Etapa 1: compilar y publicar -----------------------------------------
# Usa la imagen del SDK (pesada, ~800 MB) solo para compilar. No es la
# imagen que termina corriendo en producción.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos primero solo los .csproj de cada proyecto. Mientras las
# dependencias (PackageReference/ProjectReference) no cambien, Docker
# reutiliza esta capa cacheada y no vuelve a correr `restore` en cada build,
# aunque hayas editado un .cs.
COPY src/StreamFlix.Domain/StreamFlix.Domain.csproj src/StreamFlix.Domain/
COPY src/StreamFlix.Application/StreamFlix.Application.csproj src/StreamFlix.Application/
COPY src/StreamFlix.Infrastructure/StreamFlix.Infrastructure.csproj src/StreamFlix.Infrastructure/
COPY src/StreamFlix.Api/StreamFlix.Api.csproj src/StreamFlix.Api/
RUN dotnet restore src/StreamFlix.Api/StreamFlix.Api.csproj

# Recién ahora copiamos el resto del código fuente y publicamos.
COPY src/ src/
RUN dotnet publish src/StreamFlix.Api/StreamFlix.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Etapa 2: imagen final --------------------------------------------
# Imagen de runtime (~200 MB): no incluye el SDK ni el código fuente,
# solo lo que hace falta para ejecutar la app ya compilada.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Npgsql intenta negociar autenticación GSSAPI/Kerberos al conectar, y esta
# imagen no trae esa librería nativa por defecto. Sin ella igual funciona
# (Npgsql cae de nuevo a autenticación por contraseña), pero tira un
# "Error: ... cannot open shared object file" en cada arranque. La instalamos
# para tener logs limpios.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Dentro del contenedor la app escucha en HTTP puro (sin certificado de
# desarrollo): Kestrel expone el puerto 8080, y docker-compose.yml lo
# mapea hacia afuera. HTTPS queda para cuando corras la API desde el IDE.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StreamFlix.Api.dll"]
