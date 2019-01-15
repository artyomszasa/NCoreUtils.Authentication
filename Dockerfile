FROM microsoft/dotnet:2.0-sdk AS build-env-oauth2
WORKDIR /app

# COPY PROJECT FILES

COPY ./NCoreUtils.Authentication.Abstractions/NCoreUtils.Authentication.Abstractions.csproj ./NCoreUtils.Authentication.Abstractions/NCoreUtils.Authentication.Abstractions.csproj
COPY ./NCoreUtils.Authentication.Local.Abstractions/NCoreUtils.Authentication.Local.Abstractions.csproj ./NCoreUtils.Authentication.Local.Abstractions/NCoreUtils.Authentication.Local.Abstractions.csproj
COPY ./NCoreUtils.Authentication.Password.Abstractions/NCoreUtils.Authentication.Password.Abstractions.csproj ./NCoreUtils.Authentication.Password.Abstractions/NCoreUtils.Authentication.Password.Abstractions.csproj
COPY ./NCoreUtils.Authentication/NCoreUtils.Authentication.csproj ./NCoreUtils.Authentication/NCoreUtils.Authentication.csproj
COPY ./NCoreUtils.Authentication.Local/NCoreUtils.Authentication.Local.csproj ./NCoreUtils.Authentication.Local/NCoreUtils.Authentication.Local.csproj
COPY ./NCoreUtils.Authentication.Password/NCoreUtils.Authentication.Password.csproj ./NCoreUtils.Authentication.Password/NCoreUtils.Authentication.Password.csproj

COPY ./NCoreUtils.OAuth2.Abstractions/NCoreUtils.OAuth2.Abstractions.csproj ./NCoreUtils.OAuth2.Abstractions/NCoreUtils.OAuth2.Abstractions.csproj
COPY ./NCoreUtils.OAuth2.Data/NCoreUtils.OAuth2.Data.csproj ./NCoreUtils.OAuth2.Data/NCoreUtils.OAuth2.Data.csproj
COPY ./NCoreUtils.OAuth2.Core/NCoreUtils.OAuth2.Core.csproj ./NCoreUtils.OAuth2.Core/NCoreUtils.OAuth2.Core.csproj
COPY ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/NCoreUtils.OAuth2.Data.EntityFrameworkCore.csproj ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/NCoreUtils.OAuth2.Data.EntityFrameworkCore.csproj
COPY ./NCoreUtils.OAuth2.Encryption.Google/NCoreUtils.OAuth2.Encryption.Google.csproj ./NCoreUtils.OAuth2.Encryption.Google/NCoreUtils.OAuth2.Encryption.Google.csproj
COPY ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.csproj ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.csproj

RUN dotnet restore ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.csproj -r linux-x64

# COPY SOURCE CODES
COPY ./NCoreUtils.Authentication.Abstractions/*.cs ./NCoreUtils.Authentication.Abstractions/
COPY ./NCoreUtils.Authentication.Local.Abstractions/*.cs ./NCoreUtils.Authentication.Local.Abstractions/
COPY ./NCoreUtils.Authentication.Password.Abstractions/*.cs ./NCoreUtils.Authentication.Password.Abstractions/
COPY ./NCoreUtils.Authentication/*.cs ./NCoreUtils.Authentication/
COPY ./NCoreUtils.Authentication.Local/*.cs ./NCoreUtils.Authentication.Local/
COPY ./NCoreUtils.Authentication.Password/*.cs ./NCoreUtils.Authentication.Password/

COPY ./NCoreUtils.OAuth2.Abstractions/*.cs ./NCoreUtils.OAuth2.Abstractions/
COPY ./NCoreUtils.OAuth2.Data/*.cs ./NCoreUtils.OAuth2.Data/
COPY ./NCoreUtils.OAuth2.Core/*.cs ./NCoreUtils.OAuth2.Core/
COPY ./NCoreUtils.OAuth2.Core/Data ./NCoreUtils.OAuth2.Core/
COPY ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/*.cs ./NCoreUtils.OAuth2.Data.EntityFrameworkCore/
COPY ./NCoreUtils.OAuth2.Encryption.Google/*.cs ./NCoreUtils.OAuth2.Encryption.Google/
COPY ./NCoreUtils.OAuth2.WebService/*.cs ./NCoreUtils.OAuth2.WebService/
COPY ./NCoreUtils.OAuth2.WebService/Controllers ./NCoreUtils.OAuth2.WebService/
COPY ./NCoreUtils.OAuth2.WebService/ViewModel ./NCoreUtils.OAuth2.WebService/

RUN dotnet publish ./NCoreUtils.OAuth2.WebService/NCoreUtils.OAuth2.WebService.csproj -c Release -r linux-x64 --no-restore -o /app/out

# **********************************************************************************************************************
# RUNTIME IMAGE

FROM microsoft/dotnet:2.0-runtime-deps
WORKDIR /app
# install curl
RUN apt-get update && apt-get install -y curl
# setup environment
ENV ASPNETCORE_ENVIRONMENT=Production
# copy app
COPY --from=build-env-oauth2 /app/out ./
# copy appsettings template and entrypoint script
COPY ./appsettings.json.template ./entrypoint.sh ./
# create entry point
ENTRYPOINT ["/bin/bash", "./entrypoint.sh"]
