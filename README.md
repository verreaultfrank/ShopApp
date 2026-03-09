# JobHunter Dashboard 🏭

> **A rapid agentic programming experiment** — This entire project was built in approximately 10 hours as a personal challenge to practice and test agentic AI programming using my weekly free tokens for **Gemini 3.1 Pro** and **Claude Opus 4.6**. I'm genuinely amazed by the sheer volume of work accomplished in such a short time, and particularly impressed by how well the models follow structured instructions using the new **SKILLS paradigm** — a custom agent instruction system that dramatically improved consistency and code quality throughout development. I'm completely hooked — and this experiment has convinced me to build a **local AI setup** to run the biggest open-source models like **Qwen3 Coder** for effectively infinite tokens, because even a Pro subscription runs dry fast when you're doing this kind of agentic, multi-conversation work. Untill then I'll be using the free tokens I get weekly to improve this project.

---

## 📋 Overview

**JobHunter Dashboard** is a **.NET MAUI** desktop application designed to help a machining shop manage job leads, track their status pipeline, manage inventory, and analyze business opportunities. It is built to production-grade architectural standards despite the extremely compressed timeline.

The project was built as a full-stack solution targeting a local **SQL Server** database, featuring a rich UI, a clean layered architecture, and domain-driven design.

---

## 🏗️ Architecture

The solution follows **Clean Architecture** and **Domain-Driven Design (DDD)** principles, enforced throughout development by a custom AI skill (`clean_architecture_ddd`). The project is split into four distinct layers:

```
ShopApp/
├── JobHunter.Domain/          # Pure C# domain models & repository interfaces
├── JobHunter.Application/     # Business logic & orchestration services
├── JobHunter.Infrastructure/  # EF Core entities, DbContext, repositories, migrations
├── JobHunterDashboard/        # .NET MAUI UI — MVVM (Views + ViewModels)
└── JobHunterFileSystem/       # File system utilities (CLI tooling)
```

### Layer Responsibilities

| Layer | Responsibility |
|---|---|
| **Domain** | Pure C# models, repository interfaces, no framework dependencies |
| **Application** | Orchestrates use cases via service classes, calls repository interfaces |
| **Infrastructure** | EF Core entities, `DbContext`, concrete repositories, SQL Server migrations |
| **Dashboard** | .NET MAUI pages, MVVM ViewModels, Converters — no business logic |

---

## ✨ Features

### Job Lead Management
- Full **CRUD** for job opportunities (leads)
- Link each lead to a **Business** and a preferred **Contact**
- Track the full **status history** of every lead (e.g. New → Quoted → Won/Lost)
- Filter and sort leads by status, business, contact, and date

### Inventory Management
- Manage **Stock Items** with type categorization (`StockType` migrated to a DB entity)
- Track materials and part designs associated with each job

### Analytics
- Query and visualize job opportunity data
- Configurable analytics views via `AnalyticsQueryConfig`
- Dedicated `AnalyticsPage` with charts and summaries

### Settings & Configuration
- Application-level settings page
- Database connection managed via `appsettings.json`

### Part Designs
- Dedicated page for managing machining `PartDesign` records

---

## 🗄️ Domain Models

The core domain models living in `JobHunter.Domain/Models/`:

- **`JobOpportunity`** — A job lead with pricing, dates, metadata, materials, and status history
- **`LeadStatusHistory`** — Immutable status change records for a lead's full lifecycle
- **`JobStatus`** — Status definitions (migrated from enum to a database-backed entity)
- **`Business`** — A company/client entity linked to leads
- **`Contact`** — An individual contact associated with a business
- **`StockItem`** / **`StockType`** — Inventory items and their categories
- **`Material`** / **`PartDesign`** — Manufacturing-related sub-entities
- **`MachiningProcess`** — Enum of inferred machining processes on a lead

---

## 🤖 AI Agent Skills System

A key part of this experiment was the use of a custom **SKILLS paradigm** — a set of `.md` instruction files placed in `.agents/skills/` that guide the AI agent's behavior:

| Skill | Purpose |
|---|---|
| `clean_architecture_ddd` | Enforces layered architecture, DDD patterns, and MVVM in every code change |
| `update_database_schema` | Ensures the SQL Server schema is kept in sync whenever a C# model changes |
| `minimize_terminal_usage` | Prevents excessive token consumption from terminal output — prompts the user to run commands manually |

These skills acted as **persistent, reusable instructions** that kept the AI consistent across the many separate conversations that made up this ~10-hour sprint.

---

## 🛠️ Tech Stack

| Technology | Role |
|---|---|
| **.NET 9 / MAUI** | Cross-platform desktop UI framework |
| **C#** | Primary language |
| **Entity Framework Core** | ORM for data access |
| **SQL Server** | Relational database |
| **MVVM Pattern** | UI architecture (ViewModels, Commands, Bindings) |
| **Gemini 2.5 Pro** | AI agent for architecture, domain modeling, and planning |
| **Claude Opus 4.6** | AI agent for rapid implementation and refactoring |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [.NET MAUI workload](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation)
- SQL Server (local or remote)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/ShopApp.git
   cd ShopApp
   ```

2. **Configure the database connection**

   Edit `JobHunterDashboard/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=JobHunterDb;Trusted_Connection=True;"
     },
     "JobHunterFileSystem": "Root path to the application folder"
   }
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update --project JobHunter.Infrastructure --startup-project JobHunterDashboard
   ```

4. **Run the application**
   ```bash
   dotnet run --project JobHunterDashboard
   ```

---

## 📁 Project Structure (Detailed)

```
JobHunter.Domain/
├── Models/
│   ├── JobOpportunity.cs
│   ├── LeadStatusHistory.cs
│   ├── JobStatus.cs
│   ├── Business.cs
│   ├── Contact.cs
│   ├── StockItem.cs
│   ├── StockType.cs
│   ├── Material.cs
│   ├── PartDesign.cs
│   └── MachiningProcess.cs
└── Interfaces/
    ├── IJobLeadRepository.cs
    ├── IJobOpportunityRepository.cs
    ├── IBusinessRepository.cs
    ├── IJobStatusRepository.cs
    ├── IStockItemRepository.cs
    ├── IStockTypeRepository.cs
    ├── IMaterialRepository.cs
    ├── IPartDesignRepository.cs
    └── IAnalyticsRepository.cs

JobHunter.Application/
├── Services/
│   ├── LeadWorkflowService.cs
│   ├── BusinessService.cs
│   ├── StockItemService.cs
│   ├── StockTypeService.cs
│   └── AnalyticsService.cs
└── Interfaces/

JobHunter.Infrastructure/
├── Entities/             # EF Core entity classes
├── Repositories/         # Concrete repository implementations
├── Migrations/           # EF Core SQL migration history
├── JobHunterDbContext.cs
└── JobHunterDbContextFactory.cs

JobHunterDashboard/
├── Views/
│   ├── MainPage.xaml          # Lead management UI
│   ├── InventoryPage.xaml     # Stock & inventory UI
│   ├── AnalyticsPage.xaml     # Analytics & reporting UI
│   ├── PartDesignsPage.xaml
│   ├── JobsPage.xaml
│   └── SettingsPage.xaml
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── InventoryViewModel.cs
│   ├── AnalyticsViewModel.cs
│   ├── PartDesignsViewModel.cs
│   └── SettingsViewModel.cs
└── Converters/
```

---

## 💡 Reflections on Agentic Development

This project was a proof of concept for **agentic programming** — letting AI models drive the majority of the coding while the developer acts as an architect and reviewer.

Key takeaways:
- **SKILLS files dramatically improve consistency**: Having the agent re-read structured instructions at the start of each task prevented architectural drift across dozens of separate conversations.
- **The models handle complex refactoring well**: Multi-layer changes (domain → infrastructure → application → UI) were handled reliably when the architecture was clearly defined upfront.
- **Token discipline matters**: The `minimize_terminal_usage` skill was surprisingly effective at reducing wasted tokens on long CLI outputs.
- **10 hours, production-grade architecture**: The combination of Gemini 2.5 Pro for planning and Claude Opus 4.6 for implementation proved to be a powerful pairing.

---

*Built with curiosity, free tokens, and way too much coffee. ☕*
