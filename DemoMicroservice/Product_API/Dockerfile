FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Product_API/Product_API.csproj", "Product_API/"]
RUN dotnet restore "Product_API/Product_API.csproj"
COPY . .

WORKDIR "/src/Product_API"
RUN dotnet build "Product_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Product_API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "Product_API.dll"]