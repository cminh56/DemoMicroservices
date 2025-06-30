# Order_API/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Order_API/Order_API.csproj", "Order_API/"]
RUN dotnet restore "Order_API/Order_API.csproj"
COPY . .
WORKDIR "/src/Order_API"
RUN dotnet build "Order_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Order_API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5003
ENTRYPOINT ["dotnet", "Order_API.dll"]