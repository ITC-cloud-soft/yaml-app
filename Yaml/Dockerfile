# 使用 ASP.NET Core SDK 镜像作为构建环境
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /app

COPY . ./

# 还原所有 NuGet 包
RUN dotnet restore ./Yaml.csproj

# 构建
RUN dotnet publish -c Release -o out

# 生成最终镜像
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "YAML.dll"]
