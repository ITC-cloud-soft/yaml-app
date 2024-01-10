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


#FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build
#
#COPY CCFlow/. /source/CCFlow/
#COPY RefDLL/. /source/RefDLL/
#COPY *.sln /source/
#
#WORKDIR /source
#
#RUN dotnet restore -r linux-musl-x64
#
#RUN rm -fr  ./CCFlow/bin
#RUN rm -fr  ./CCFlow/obj
#RUN dotnet build ./CCFlow -o ./CCFlow/bin/Release/netcoreapp3.1/publish/
#RUN cp -r ./CCFlow/wwwroot ./CCFlow/bin/Release/netcoreapp3.1/publish/
#
#FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
#
#RUN apt-get update -y
#RUN apt-get install libgdiplus -y
#
#COPY --from=build /source/CCFlow/bin/Release/netcoreapp3.1/publish/ /App
#
#WORKDIR /App
#
#
#EXPOSE 80
#
#ENV ASPNETCORE_URLS http://+:80
#
##時刻をアジア／東京に設定する。デフォルトはUTC
#ENV TZ=Asia/Tokyo
#RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
#
#RUN echo '23.12.14.3'
#RUN apt-get update -y && apt-get upgrade -y && rm -rf /var/lib/apt/lists/*
#RUN apt-get upgrade openssl -y
#
##ブートストラップ
#ENTRYPOINT ["dotnet", "CCFlow.NetCore.dll"]