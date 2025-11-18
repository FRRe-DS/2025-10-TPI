# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SolutionCompras/ComprasAPI/ComprasAPI.csproj", "SolutionCompras/ComprasAPI/"]
RUN dotnet restore "SolutionCompras/ComprasAPI/ComprasAPI.csproj"

COPY . .
WORKDIR "/src/SolutionCompras/ComprasAPI"
RUN dotnet build "ComprasAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ComprasAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "ComprasAPI.dll"]