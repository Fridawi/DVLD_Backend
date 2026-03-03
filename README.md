# DVLD System

A multi-layered .NET 10 solution for managing driving license data.

## Solution Structure

- **DVLD.API**  
  ASP.NET Core Web API project.  
  - Reads configuration from `appsettings.json`.
  - Uses Serilog for logging (Console, Seq, File).
  - Supports JWT authentication.
  - Handles allowed origins and file storage settings.

- **DVLD.EF**  
  Infrastructure and Entity Framework Core project.  
  - Contains repository and unit of work patterns.
  - Implements business services (e.g., `PersonService`).
  - Manages database context and migrations.

- **DVLD.CORE**  
  Core entities, DTOs, and interfaces.  
  - Defines domain models (e.g., `Person`, `Country`).
  - Contains DTOs for API communication.
  - Declares interfaces for repositories and services.

- **DVLD.Infrastructure.Mapping**  
  Contains mapping configuration (AutoMapper profiles).

- **DVLD.Tests**  
  Unit test project.  
  - Uses xUnit and Moq for testing.
  - Organized by layer: Services, Controllers, Repositories.

## Features

- Layered architecture for separation of concerns.
- Logging with Serilog (Console, Seq, File).
- JWT authentication for secure endpoints.
- File storage configuration.
- Unit tests for business logic and API endpoints.

## Getting Started

1. **Clone the repository:**