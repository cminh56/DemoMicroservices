FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Basket_API/Basket_API.csproj", "Basket_API/"]
RUN dotnet restore "Basket_API/Basket_API.csproj"
COPY . .
WORKDIR "/src/Basket_API"
RUN dotnet build "Basket_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Basket_API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5004
ENTRYPOINT ["dotnet", "Basket_API.dll"] 