# 使用 ASP.NET Core SDK 镜像作为构建环境
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /app

# Copy csproj and restore as distinct layers to cache dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out 

# 生成最终镜像
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "YAML.dll"]
