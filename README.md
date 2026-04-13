🧠 1. Inventory Hold System

Backend architecture (DDD + microservices thinking)
Data consistency (MongoDB atomic ops)
System design (Redis + RabbitMQ integration)
Real-world flows (inventory locking/holds)
Full-stack capability (React + API sync)
Engineering maturity (testing + Docker + AI usage)

👉 Translation: They want production-ready thinking, not just code.

🏗️ 2. Final Repo Structure (You Should Create)

This is your exact GitHub structure 👇

inventory-hold-system/
│
├── docker-compose.yml
├── README.md
├── AI-USAGE.md
│
├── src/
│   ├── InventoryHold.Contracts/
│   │   ├── DTOs/
│   │   ├── Enums/
│   │   └── Events/
│   │
│   ├── InventoryHold.Domain/
│   │   ├── Entities/
│   │   ├── Services/
│   │   └── Repositories/
│   │
│   ├── InventoryHold.Infrastructure/
│   │   ├── Mongo/
│   │   ├── Redis/
│   │   ├── Messaging/
│   │   └── Config/
│   │
│   ├── InventoryHold.WebApi/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Extensions/
│   │   └── Program.cs
│   │
│   └── InventoryHold.UnitTests/
│       ├── Services/
│       └── Mocks/
│
├── frontend/
│   ├── src/
│   ├── components/
│   ├── services/
│   └── types/
│
└── docker/
    ├── api.Dockerfile
    └── frontend.Dockerfile
🔄 3. System Flow (VERY IMPORTANT – Use This in README)

Here’s your end-to-end flow 👇

User → React UI → API → Domain Service
     → MongoDB (atomic update)
     → Redis (cache)
     → RabbitMQ (event publish)
🔥 Hold Creation Flow
User clicks Create Hold
API checks inventory
MongoDB:
Atomically reduces stock
Creates hold with expiry
Redis:
Cache inventory
RabbitMQ:
Publish HoldCreated

🧩 4. Core Backend Design
📌 Entities (Domain Layer)
Product
Inventory
Hold
HoldItem
📌 Key Services
HoldService (MAIN logic)
InventoryService
ExpirationService (optional bonus 🔥)
📌 Repository Interfaces
IInventoryRepository
IHoldRepository
📌 Infrastructure
MongoDB → persistence
Redis → caching
RabbitMQ → events

🌐 5. API Design (Must Match Exactly)
✅ Holds
POST   /api/holds
GET    /api/holds/{id}
DELETE /api/holds/{id}
✅ Inventory
GET /api/inventory

⚡ 6. Critical Logic (Interview Gold 🔥)
🧠 Atomic Inventory Update (IMPORTANT)

From requirements :

👉 Use MongoDB:

FindOneAndUpdate with condition:
quantity >= requested

If fails → return 409 Conflict

⏳ Expiry Handling
Store:
ExpiresAt = CreatedAt + 15 min
On GET:
If expired → treat as invalid
Publish HoldExpired

📡 7. Messaging Design (RabbitMQ)
Events:
HoldCreated
HoldReleased
HoldExpired
Event Example:
{
  "holdId": "123",
  "items": [],
  "status": "CREATED",
  "timestamp": "..."
}

⚡ 8. Caching Strategy (Redis)

Cache:

/api/inventory (HIGH priority)
Rules:
TTL: 30–60 sec
Invalidate cache on:
Hold create
Hold release

🧪 9. Unit Tests (MANDATORY)

Minimum 5 tests:

✅ Valid hold creation
❌ Insufficient inventory
❌ Expired hold
❌ Double release
⚡ Concurrency test

👉 Mock:

Mongo repo
Redis
RabbitMQ

🎨 10. Frontend (React + TypeScript)
Pages:
Inventory Dashboard
Create Hold
Active Holds
Release Hold
Key Requirement:

👉 UI must auto-update after API calls (no refresh)

🚀 11. Execution Plan (2-Day Strategy)
Day 1:
Backend (API + Mongo + basic logic)
Docker setup
Day 2:
Redis + RabbitMQ
Frontend
Tests + README + AI-USAGE
Startup:

  docker-compose up --build

Local setup:

1. Run `dotnet restore` at the solution root.
2. Install frontend dependencies: `cd frontend && npm install`.

If Visual Studio shows missing package assets, run the included script `scripts/setup.ps1` from powershell to restore both .NET and npm packages.

See AI-USAGE.md for details about AI assistance used.


🐳 12. Docker Setup
docker-compose must include:
API
MongoDB
Redis
RabbitMQ

Run:
docker-compose up --build

📄 13. README Structure

Include:

Architecture diagram (use flow above)
Setup steps
API endpoints
Design decisions
Trade-offs
