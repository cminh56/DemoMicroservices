FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Inventory_API/Inventory_API.csproj", "Inventory_API/"]
RUN dotnet restore "Inventory_API/Inventory_API.csproj"
COPY . .
WORKDIR "/src/Inventory_API"
RUN dotnet build "Inventory_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Inventory_API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inventory_API.dll"] 