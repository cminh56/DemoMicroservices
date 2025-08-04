# Identity_API/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Identity_API/Identity_API.csproj", "Identity_API/"]
RUN dotnet restore "Identity_API/Identity_API.csproj"
COPY . .
WORKDIR "/src/Identity_API"
RUN dotnet build "Identity_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity_API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5007
ENTRYPOINT ["dotnet", "Identity_API.dll"] 