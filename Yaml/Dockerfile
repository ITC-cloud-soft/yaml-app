# 使用 ASP.NET Core SDK 镜像作为构建环境
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# 设置工作目录
WORKDIR /app

# 复制 csproj 并还原为不同层以缓存依赖项
COPY *.csproj ./
RUN dotnet restore

# 复制其他文件并构建
COPY . ./
RUN dotnet publish -c Release -o out 

# 生成最终镜像
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# 设置工作目录
WORKDIR /app

# 复制发布的应用程序
COPY --from=build-env /app/out .

# 安装 bash 和 kubectl
RUN apt-get update && apt-get install -y bash curl \
    && curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl" \
    && install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl 

# 确保 /app/data 目录存在，并将其设置为工作目录
RUN mkdir -p /app/data
VOLUME /app/data

# 设置环境变量以指示 SQLite 数据库文件的位置
ENV SQLite__ConnectionStrings__DefaultConnection "Data Source=/app/data/app.db"

# 入口点
ENTRYPOINT ["dotnet", "Yaml.dll"]
