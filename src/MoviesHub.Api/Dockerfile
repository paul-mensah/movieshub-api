#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/MoviesHub.Api/MoviesHub.Api.csproj", "src/MoviesHub.Api/"]
RUN dotnet restore "src/MoviesHub.Api/MoviesHub.Api.csproj"
COPY . .
WORKDIR "/src/src/MoviesHub.Api"
RUN dotnet build "MoviesHub.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MoviesHub.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MoviesHub.Api.dll"]