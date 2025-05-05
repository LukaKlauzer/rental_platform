### Branch Overview

- **master**  
  Contains the main implementation without API authorization or Mediator.

- **API-auth**  
  Contains everything from `master`, with the addition of basic authorization.  
  Authorization works via the `api/customer/login/{id}` endpoint, which returns a token.  
  While it’s not full authentication, it should be sufficient for testing purposes.  
  When using Swagger (note: avoid Docker if you want to use Swagger), click the **Authorize** button in the top right corner, paste in the token, and you’ll be able to access the protected endpoints.  
  Account creation and login work regardless of whether a token is provided.

- **Mediator**  
  Contains everything from both the `master` and `API-auth` branches, as it was built on top of `API-auth`.

I decided to keep these features on separate branches because adding API authorization would break integration tests, and adding Mediator would break unit tests.

# Rental Platform

## How to Run the Application

### From Visual Studio (or equivalent):

1. Run the "rental_platform" project to create the migration. (should be different in production, but it is good enough here)
   - There will be no data for vehicles and telemetry initially.
2. Run the Seed project and wait for it to complete.
3. Run the "rental_platform" project again.
   - Visit https://localhost:7014/health to see "Healthy" and check that https://localhost:7014/swagger is available.

### Using Docker:

1. Run the Docker Compose file.
   - Wait for the process to complete.

## What will happen when using Docker?

1. The DB container will start, and everything else will pause until the database is 
   fully ready to accept connections. (Health check -> assures applications don't 
   timeout while trying to connect)
2. The API project will start and execute migrations. Once it's fully ready 
   (Health check on endpoint /health)
3. The Seeder will run and populate the database with necessary data. Afterward, it will shut down.

You can access Swagger at https://localhost:8080/swagger

## Known Issues with Docker (In my implementetion!):

- Unfortunately, when running through Docker, the application is not available over HTTPS 
  due to a certificate issue. Docker generates a certificate, but Kestrel is not 
  currently configured to look for it (in Program.cs of Api project). This issue 
  arises because it would cause problems if the app were run through Visual Studio. 
  It could be resolved by detecting the Docker environment and only looking for the 
  certificate then, but that would require additional work on the Docker configuration 
  and Program.cs.
    
- Detecting docker environment and setting swagger slightly differently would solve the 
  problem of not being able to execute API calls from swagger as well.

## Architecture

I opted for clean architecture since it completely separates infrastructure from business logic:

### 1. Core

Contains: Entities, DTOs, interfaces necessary to implement business logic in Application 
layer (and ceeder project in this case, but more on that in section 3.) and data 
access in infrastructure layer, and some helpers like extensions for entity to DTO 
conversion and vice versa (I later discovered this could be done in a better and 
more convenient way by utilizing records, but since there was limited time...)

### 2. Application

Implements all the business logic like creating reservations while checking for overlapping 
data. As of writing of this document (30.4.2025/1.5.2025 at 8:30 AM) it did not 
implement Mediator, but it is in my plan to do so if I have time. I would place 
implementation of that pattern in this project/layer.

### 3. Infrastructure

Completely separated from business logic and can be replaced with different implementation 
at any time. Implements generic repository that allows me to write a wrapper around 
any of the entities, and I don't have to think of implementing CRUD all over again.
Contains everything related to DB, context and migrations.

If instead of a csv-s there was an API or some other way of gathering data related 
to telemetry, it would most likely be implemented here (interface: ITelemetryDataProvider)

### 4. Api

Contains controllers for customers, vehicles and reservation. Besides that, it contains 
extension on controller. That extension encapsulates interpretation of Error or 
result that is carried in result pattern and converts it to appropriate IActionResult.

### 5. Seeder

I developed it as a console app, since it has to be run only once. It references the 
core project and implements necessary interfaces to perform required business logic 
(get data, validate data and at the end, utilize infrastructure to persist the data).
        
Console application whose purpose is to acquire the data (from CSV), validate it and 
insert into the DB.
        
Implements interfaces from core (ITelemetryDataProvider and IVehicleDataProvider) - 
in this case data is delivered through CSV files so I implemented CsvTelemetryDataProvider 
and CsvVehicleDataProvider. 

These "services" validate headers, read data, validate it by utilizing corresponding 
validation services (defined in core, and implemented in seeder) and after that, check 
if data already exists in DB. For that purpose I created an index on telemetry data 
(Vin, TelemetryType & timestamp).

Regarding migrations... Process of starting application is not the most convenient 
(run api, run seeder and then api again) and should be handled differently in production. 
It could be better to delegate it to the seeder, but I wanted to keep its functionality 
to handling validation and insertion of data in DB.

### 6. Unit test

Contains unit tests :)
I utilized mocking in order to set up external services (services that are not subject 
of tests) to orchestrate some possible use cases.
It is a straightforward process since architecture is clean!

### 7. Integration test

Implements CustomWebApplicationFactory. That allows us to intercept services and 
remove/add services from service pool.
So I removed every service related to standard database and injected in-memory 
database. This is particularly useful when running integration tests as part of 
the pipeline.


## Note on Implementation (Updated 05.05.2025 - 7:38)

I would like to clarify where I would implement the logic used in the seeder if the project was not a "demo". As mentioned in section 3, Infrastructure would hold implementation of ITelemetryDataProvider since it falls into the "data gathering category". 

Unlike in my current implementation, ITelemetryValidator & IVehicleValidator would be implemented in the application layer since they would be making business decisions whether to accept the data or not. However, since the seeder is only run once, I decided to implement these interfaces in the seeder itself for simplicity.

