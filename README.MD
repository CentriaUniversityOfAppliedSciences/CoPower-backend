Dashboard API based on MajakkaAPI

# Production server (Ubuntu 22.04)

# Install PostgreSQL
sudo apt install postgresql

## Install Dotnet runtime and sdk
- sudo apt install dotnet-sdk-10.0
- sudo apt install dotnet-runtime-10.0
- sudo apt install aspnetcore-runtime-10.0

## Install Dotnet dependancies
dotnet dev-certs https

## Install server port environment variable
export ASPNETCORE_URLS="http://localhost:5100;https://localhost:5101;http://192.168.56.2:5100;https://192.168.56.102:5101"

## Set "env.json" environment variable file with the keys listed in "env_example.json"

## Run server
dotnet Copower-Dashboard-API.dll

# Development (Windows)

## Set "env.json" environment variable file with the keys listed in "env_example.json"

## Install PostgreSQL
https://www.enterprisedb.com/downloads/postgres-postgresql-downloads

- Run database.sql script to create users and databases, instructions on how to run the file is at the start of this file

## Install Visual Studio Community Edition
https://visualstudio.microsoft.com/vs/community/ then open the project .sln file with it

## Install NuGet packages with NuGet Package Manager (Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...) if not installed
- Azure.Identity
- BCrypt.Net-Next
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.Graph
- Newtonsoft.Json
- Npgsql.EntityFrameworkCore.PostgreSQL
- Swashbuckle.AspNetCore
