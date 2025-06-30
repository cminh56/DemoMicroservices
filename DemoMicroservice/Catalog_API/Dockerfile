FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Catalog_API/Catalog_API.csproj", "Catalog_API/"]
RUN dotnet restore "Catalog_API/Catalog_API.csproj"
COPY . .

WORKDIR "/src/Catalog_API"
RUN dotnet build "Catalog_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Catalog_API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5005
ENTRYPOINT ["dotnet", "Catalog_API.dll"] 