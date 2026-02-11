# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy solution + project files first (better cache)
COPY PointOfSale/PointOfSale.sln ./PointOfSale/
COPY PointOfSale/PointOfSale/*.csproj ./PointOfSale/PointOfSale/
COPY PointOfSale/PointOfSale.Business/*.csproj ./PointOfSale/PointOfSale.Business/
COPY PointOfSale/PointOfSale.Data/*.csproj ./PointOfSale/PointOfSale.Data/
COPY PointOfSale/PointOfSale.Model/*.csproj ./PointOfSale/PointOfSale.Model/

# Restore using the solution
RUN dotnet restore ./PointOfSale/PointOfSale.sln

# Copy everything else
COPY . .

# Publish the web project
RUN dotnet publish ./PointOfSale/PointOfSale/PointOfSale.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

# Native deps for wkhtmltox (DinkToPdf) on Debian-based images
RUN apt-get update \
	&& apt-get install -y --no-install-recommends \
		libx11-6 \
		libxext6 \
		libxrender1 \
		libfontconfig1 \
		libfreetype6 \
		libjpeg62-turbo \
		fonts-dejavu-core \
		ca-certificates \
	&& rm -rf /var/lib/apt/lists/*

# Render sets PORT; locally we pass PORT ourselves
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PointOfSale.dll"]
