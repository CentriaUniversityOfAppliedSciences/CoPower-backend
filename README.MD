# CoPower-Backend (version 0.9)

![Project is co-funded by the European Union](funder-logo-en.png)
[Project Website](https://www.interreg-npa.eu/projects/copower/home/)

## Minimum requirements:

### [Frontend](https://github.com/CentriaUniversityOfAppliedSciences/CoPower)
```
Node.js (24 LTS)
Vue.js (3.5)
```

### [Backend (this repository)](https://github.com/CentriaUniversityOfAppliedSciences/CoPower-backend)
```
PostgreSQL (16)
.NET (10)
```

### [Services](https://github.com/CentriaUniversityOfAppliedSciences/CoPower-services)
```
Python 3 (3.14)
```

## Project structure
Project is split into 3 parts
- [Frontend](https://github.com/CentriaUniversityOfAppliedSciences/CoPower)
- This repository is for the backend.
- [Services](https://github.com/CentriaUniversityOfAppliedSciences/CoPower-services)

## How to get started

1. Clone the repository
2. Create the initial database structure and users by running **CreateInitialDatabase.sql**, if the databases does not exist. SQL script creates default user and organisation, they can be changed by updating the SQL file before running it.
3. Create appsettings.json and env.json files based on the example files provided under **Copower-Dashboard-API** folder.
4. Open the **Copower-Dashboard-API.sln** project file with Microsoft Visual Studio (minimum 2026 version required)

## Building for production
1. Open Microsoft Visual Studio 2026
2. From the top menu click "Publish" and then "Publish Copower-Dashboard-API" and publish the build
3. After published build has been finished copy the files to the destination folder and run **Copower-Dashboard-API.exe** (Windows) or **Copower-Dashboard-API.dll** (Linux) to start the server.


# Contribution Guidelines
## Missing a feature or found a bug?
Please submit it as an issue to this repository with as much explanation about the bug or feature as possible. In case of a security, do not create a new issue, send email about it to ville.peltola@centria.fi.

## License: MIT
See LICENSE for more information.

## Developers
Centria University of Applied Sciences Ltd.
- Ville Peltola (Developer)
- Fabian Sander (Project Manager)