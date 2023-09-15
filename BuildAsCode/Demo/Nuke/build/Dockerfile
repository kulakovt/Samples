ARG REPO=mcr.microsoft.com/dotnet
ARG SDK=${REPO}/sdk:6.0
ARG RUN=${REPO}/aspnet:6.0

##################### Installer #####################

FROM ${RUN} AS installer

ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

RUN groupadd --gid 999 dotnet && \
    useradd --create-home --home-dir /app --uid 999 --gid dotnet dotnet

USER 999:999

##################### Build #####################

FROM ${SDK} AS build

ARG Version=0.0.0.0
ARG AssemblyVersion=0.0.0.0
ARG FileVersion=0.0.0.0
ARG InformationalVersion=0.0.0.0

# Enable integration with TeamCity (in tests)
ENV TEAMCITY_VERSION=0

RUN echo Application Version: ${Version}

WORKDIR /app/src
COPY . .

RUN dotnet restore

RUN dotnet build \
    --configuration Release \
    --no-restore \
    --nologo \
    -p:Version="${Version}" \
    -p:AssemblyVersion="${AssemblyVersion}" \
    -p:FileVersion="${FileVersion}" \
    -p:InformationalVersion="${InformationalVersion}"

##################### Test #####################

FROM build AS test

RUN dotnet test \
    --filter "Category!=integration" \
    --configuration Release \
    # xunit/issues/1706
    --verbosity normal \
    --no-build \
    --no-restore \
    --nologo
    # TODO: Add coverlet.collector to projects
    # --collect:"XPlat code coverage"

##################### Publish #####################

FROM test AS publish

RUN dotnet pack \
    --configuration Release \
    --output "/app/artifacts/nuget" \
    --no-build \
    --no-restore \
    --nologo \
    -p:PackageVersion="${Version}"

RUN dotnet publish \
    "./src/Buldac/Buldac.csproj" \
    --configuration Release \
    --output "/app/bin" \
    --no-build \
    --no-restore \
    --nologo

##################### Final #####################

FROM installer AS final

WORKDIR /app
COPY --from=publish /app/bin .

# TODO: Remove artifacts from Final image
COPY --from=publish /app/artifacts /app/artifacts

ENTRYPOINT ["dotnet", "Buldac.dll"]
