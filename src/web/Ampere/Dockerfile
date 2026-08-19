# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/web/Directory.Build.props", "src/web/"]
COPY ["src/web/Directory.Build.targets", "src/web/"]
COPY ["src/web/Ampere/Ampere.csproj", "src/web/Ampere/"]
COPY ["src/web/Ampere.Application/Ampere.Application.csproj", "src/web/Ampere.Application/"]
COPY ["src/web/Ampere.Domain/Ampere.Domain.csproj", "src/web/Ampere.Domain/"]
COPY ["src/web/Ampere.Composition/Ampere.Composition.csproj", "src/web/Ampere.Composition/"]
COPY ["src/web/Ampere.Infrastructure/Ampere.Infrastructure.csproj", "src/web/Ampere.Infrastructure/"]
RUN dotnet restore "./src/web/Ampere/Ampere.csproj"
COPY . .
WORKDIR "/src/src/web/Ampere"
RUN dotnet build "./Ampere.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ampere.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ampere.dll"]