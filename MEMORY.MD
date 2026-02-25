# PolaperBotV2 - Project Planning

## Project Overview
Recreation of PolaperBot.Core.AI using the latest Microsoft Agent Framework tools and patterns.

## Tech Stack
- .NET 10 Class Library
- .NET 10 Minimal API
- SQLite (Session persistence)
- Microsoft.Agent.Framework 1.0.0-rc1
- OllamaSharp 5.4.16
- Ollama (gpt-oss:20b-cloud)

## Project Structure
```
PolaperBotV2/
├── PolaperBot.Core.AI/           # Core AI Library (.NET 10)
│   ├── Configuration/
│   │   ├── AgentInstructions.cs  # Agent system prompts
│   │   └── AgentOptions.cs       # Configuration POCOs
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs  # Main DI entry point
│   │   ├── AgentBuilderExtensions.cs       # Agent-specific DI
│   │   ├── ToolsExtensions.cs              # Tools DI
│   │   └── InstructionsExtensions.cs       # Instructions DI
│   ├── Tools/
│   │   └── MemoryTool.cs        # MEMORY.MD tool implementation
│   ├── Sessions/
│   │   └── ISessionStore.cs     # Session store interface
│   └── Services/
│       └── AgentService.cs      # Agent orchestration service
├── PolaperBot.Infra/            # Infrastructure Layer (.NET 10)
│   ├── Extensions/
│   │   └── InfraExtensions.cs   # DI extensions
│   └── Sessions/
│       └── SqliteSessionStore.cs # SQLite session persistence
├── PolaperBot.Api/              # Minimal API (.NET 10)
│   ├── Program.cs
│   ├── appsettings.json
│   └── Endpoints/
│       └── ChatEndpoints.cs     # Chat endpoints
├── MEMORY.MD                    # This file
└── PolaperBot.slnx
```

## NuGet Packages

| Project | Package | Version |
|---------|---------|---------|
| Core.AI | `Microsoft.Agents.AI` | `1.0.0-rc1` |
| Core.AI | `Microsoft.Extensions.AI` | `10.3.0` |
| Core.AI | `Microsoft.Extensions.DependencyInjection.Abstractions` | `10.0.3` |
| Core.AI | `OllamaSharp` | `5.4.16` |
| Infra | `Microsoft.Agents.AI` | `1.0.0-rc1` |
| Infra | `Microsoft.Data.Sqlite` | `10.0.2` |
| Api | `Microsoft.AspNetCore.OpenApi` | `10.0.2` |

## Implementation Tasks

| # | Task | Status |
|---|------|--------|
| 1 | Create solution + project structure | ✅ |
| 2 | Implement `ISessionStore` interface | ✅ |
| 3 | Create `MemoryTool` for MEMORY.MD operations | ✅ |
| 4 | Build DI extensions | ✅ |
| 5 | Configure Ollama provider using OllamaSharp | ✅ |
| 6 | Create API endpoints | ✅ |
| 7 | Add SQLite session persistence | ✅ |
| 8 | Implement ChatReduction with `MessageCountingChatReducer(20)` | ✅ |

## Key Design Decisions

1. **Layered Architecture**: Core.AI (abstractions) → Infra (implementations) → Api
2. **SQLite Session Persistence**: Sessions serialized to JSON and stored in SQLite
3. **Chat Reduction**: `InMemoryChatHistoryProvider` with 20-message limit
4. **DI Separation**: Each layer has its own extension methods

## API Endpoints

### POST /api/chat
```json
{ "userId": 123456789, "message": "Hola" }
→ { "response": "¡Hola!" }
```

### POST /api/chat/human (Local Testing)
```json
{ "message": "Hola" }
→ { "response": "¡Hola!" }
```
- Fixed session ID: `999999`
- **Persists across server restarts** via SQLite

### GET /api/chat/health
```
→ "PolaperBot API is running"
```

## Configuration
```json
{
  "Database": { "SqlitePath": "sessions.db" },
  "Ollama": { "Endpoint": "http://localhost:11434", "Model": "gpt-oss:20b-cloud" },
  "Agent": { "Name": "HanniAssistant", "MemoryPath": "./MEMORY.MD" }
}
```

## Session Persistence

Sessions are stored in SQLite with this schema:
```sql
CREATE TABLE Sessions (
    UserId INTEGER PRIMARY KEY,
    SessionJson TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
)
```

- `LoadOrCreateAsync(userId)`: Loads session from DB or creates new
- `SaveAsync(userId, session)`: Serializes and saves session after each message

---
*Last updated: 2026-02-25*
