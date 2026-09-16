Configured as a docker container ([[Docker-compose]]).

The .env file contains **POSTGRES_USER**, **POSTGRES_PASSWORD**, **POSTGRES_DB**, **POSTGRES_PORT**

**EntityFramework** is used to communicate with the DB. And is configured in:

```
/backend/src/Infrastructure
```
Contains:
- **AppDbContext** used to connect to the postgress container 
- **AppContextFactory** used to connect to the db for managing migrations