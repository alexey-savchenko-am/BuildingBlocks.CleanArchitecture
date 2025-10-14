# BuildingBlocks.CleanArchitecture

**Reusable building blocks for .NET applications following Clean Architecture and Domain-Driven Design (DDD) principles.**

---

## 📦 Overview

**BuildingBlocks.CleanArchitecture** is a set of libraries that implement core concepts of **Clean Architecture** and **DDD** for .NET applications. It provides abstractions and implementations for different layers:

- **Domain** — entities, aggregates, value objects, domain events.
- **Application** — commands, queries, handlers, and specifications.
- **Infrastructure** — repository implementations, integrations with external services.

These libraries are designed for **reusability** and can be distributed as NuGet packages.

---

## 🧱 Solution Structure

The solution contains four main projects:

- `BuildingBlocks.CleanArchitecture.Domain` — domain entities and business logic.
- `BuildingBlocks.CleanArchitecture.Application` — command and query handlers.
- `BuildingBlocks.CleanArchitecture.Infrastructure` — repository and integration implementations.

---

## ⚙️ Installation

To use the libraries in your project, add the corresponding NuGet packages:

```bash
dotnet add package BuildingBlocks.CleanArchitecture.Domain
dotnet add package BuildingBlocks.CleanArchitecture.Application
dotnet add package BuildingBlocks.CleanArchitecture.Infrastructure
