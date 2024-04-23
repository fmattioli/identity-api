#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y curl
WORKDIR /app
EXPOSE 8082

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Identity.Api/Identity.Api.csproj", "Identity.Api/"]
COPY ["src/Identity.IoC/Identity.IoC.csproj", "Identity.IoC/"]
COPY ["src/Identity.Application/Identity.Application.csproj", "Identity.Application/"]
COPY ["src/Identity.Data/Identity.Data.csproj", "Identity.Data/"]
RUN dotnet restore "Identity.Api/Identity.Api.csproj"
COPY . .

RUN dotnet build "src/Identity.Api/Identity.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Identity.Api/Identity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.Api.dll"]