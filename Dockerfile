# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OMS.csproj", "."]
RUN dotnet restore "OMS.csproj"
COPY . .
RUN dotnet publish "OMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Cấu hình ASP.NET Core lắng nghe cổng 8080 (cổng mặc định cho các dịch vụ container trên Render)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OMS.dll"]
