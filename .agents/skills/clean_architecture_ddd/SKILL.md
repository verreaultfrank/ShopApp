---
name: Enforce Clean Architecture and DDD
description: Guides the development and refactoring of the .NET system to ensure it strictly adheres to Clean Architecture, Domain-Driven Design (DDD), and MVVM principles.
---

# Clean Architecture & Domain-Driven Design (DDD) Enforcement

This skill ensures that all new features and refactored code within the system maintain a strict separation of concerns, avoiding tightly coupled dependencies between the database, business logic, and UI.

## When to Use This Skill
- When creating a new feature across the stack.
- When refactoring "fat" ViewModels or `.xaml.cs` code-behinds.
- When creating or modifying database entities and Entity Framework configurations.

## Core Architectural Layers (The Dependency Rule)
Dependencies MUST only point inwards. Outer layers depend on inner layers, but inner layers have NO knowledge of outer layers.

1. **Domain Layer (Innermost):** Pure C# (POCOs). Contains Domain Models, Value Objects, Domain Events, and Repository Interfaces. **NO dependencies** on EF Core, UI, or external services.
2. **Application Layer:** Contains Use Cases, Commands/Queries (CQRS), and Business Services. Depends ONLY on the Domain Layer.
3. **Infrastructure Layer (Outermost):** Contains EF Core `DbContext`, Database Entities (Persistence Models), External API clients, and the concrete implementations of Repository Interfaces.
4. **Presentation Layer (Outermost):** UI, Views (`.xaml`), Code-Behind (`.xaml.cs`), and ViewModels. Depends on the Application layer to execute business logic.

## Rules for Models
- **NEVER** use a database Entity (Persistence Model) directly in a ViewModel or View.
- **NEVER** put business logic or validation rules inside a database Entity. Db Entities are purely for mapping to SQL tables.
- Use mapping (e.g., AutoMapper, Mapster, or manual mapping functions) to safely translate data across boundaries:
  - `[Persistence Entity]` <-> `[Domain Model]` (Performed in the Infrastructure Repository)
  - `[Domain Model]` <-> `[ViewModel/DTO]` (Performed in the Application or Presentation layer)

## Refactoring / Development Checklist

### 1. Domain (Business Logic)
- [ ] Create pure C# Domain Models representing the true business rules and state.
- [ ] Define Repository interfaces (e.g., `IOrderRepository`) that return Domain Models, NOT database entities.

### 2. Infrastructure (Data Access)
- [ ] Create or update Entity Framework Database Entities (managing `[Table]`, `[Column]`, foreign keys).
- [ ] Implement the Repository interfaces defined in the Domain.
- [ ] Map DB Entities to Domain Models before returning them from the repository (and vice versa for saving).

### 3. Application (Orchestration)
- [ ] Create Business Services or Command/Query handlers to orchestrate logic.
- [ ] Inject Repository interfaces into these services via Dependency Injection.

### 4. Presentation (MVVM / UI)
- [ ] Create ViewModels that inject Application services.
- [ ] Ensure all user actions are handled via `ICommand` properties bound to the UI, not event handlers.
- [ ] Ensure `.xaml.cs` (code-behind) contains zero business logic. It should only handle UI-specific visual behavior that XAML cannot.
- [ ] Map returned Domain Models into ViewModels to format the data appropriately for display.

## Recommended Design Patterns
Use these specific patterns to support DDD and .NET MAUI Clean Architecture:

### Creational
- **Factory Method**: Use to instantiate complex Domain Entities or Aggregates, ensuring they are created in a valid state.

### Structural
- **Adapter**: Use in the Infrastructure layer to adapt external services/APIs to Domain interfaces (Ports & Adapters).
- **Facade**: Use in the Application layer to provide simplified entry points for complex business operations to the MAUI Presentation layer.

### Behavioral
- **Command**: Encapsulate MAUI UI actions (`ICommand` in MVVM ViewModels) and standardize Application use cases (CQRS).
- **Mediator**: Decouple Application layer components (e.g., MediatR) and broadcast Domain Events without direct dependencies.
- **Observer**: Essential for MAUI data binding (`INotifyPropertyChanged`) and reacting to domain state changes.
