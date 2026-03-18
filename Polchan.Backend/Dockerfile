FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:478b9038d187e5b5c29bfa8173ded5d29e864b5ad06102a12106380ee01e2e49 AS build
WORKDIR /app

# Copy project metadata first to speed up restore
# (restore won't need to redownload nuget packages if only source code changes)
COPY Polchan.Backend.slnx ./
COPY src/Polchan.Core/Polchan.Core.csproj src/Polchan.Core/
COPY src/Polchan.Application/Polchan.Application.csproj src/Polchan.Application/
COPY src/Polchan.Infrastructure/Polchan.Infrastructure.csproj src/Polchan.Infrastructure/
COPY src/Polchan.Shared/Polchan.Shared.csproj src/Polchan.Shared/
COPY src/Polchan.Web/Polchan.Web.csproj src/Polchan.Web/

RUN dotnet restore src/Polchan.Web/Polchan.Web.csproj

# Don't include tests in final image
COPY src/ src/

RUN dotnet publish src/Polchan.Web/Polchan.Web.csproj --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:a04d1c1d2d26119049494057d80ea6cda25bbd8aef7c444a1fc1ef874fd3955b
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Polchan.Web.dll"]
