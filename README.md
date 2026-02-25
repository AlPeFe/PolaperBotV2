# PolaperBot V2 🤖

> A modern AI Agent framework implementation using Microsoft Agent Framework, Ollama, and .NET 10

## Overview

PolaperBot V2 is a conversational AI assistant built with clean architecture principles and modern .NET patterns. It demonstrates professional-grade implementation of AI agents with persistent sessions, dependency injection, and layered architecture.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      PolaperBot.Api                         │
│              (Minimal API - Presentation Layer)              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   PolaperBot.Core.AI                        │
│            (Domain Layer - Abstractions & Logic)            │
│                                                             │
│  • ISessionStore      • IAgentService                       │
│  • MemoryTool         • AgentInstructions                   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    PolaperBot.Infra                         │
│           (Infrastructure Layer - Implementations)          │
│                                                             │
│  • SqliteSessionStore                                       │
└─────────────────────────────────────────────────────────────┘
```

## Tech Stack

| Category | Technology |
|----------|------------|
| **Framework** | .NET 10 |
| **AI Framework** | Microsoft Agent Framework 1.0.0-rc1 |
| **LLM Provider** | Ollama (via OllamaSharp) |
| **Database** | SQLite |
| **API Style** | Minimal API |
| **Architecture** | Clean Architecture / Onion Architecture |
| **DI Pattern** | Extension Methods per Layer |

## Key Features

### 🧠 AI Agent with Tools
- Custom tool implementation (`MemoryTool`) for persistent memory
- Extensible tool system via `AITool[]` registration
- Natural language processing in Spanish

### 💾 Session Persistence
- SQLite-backed session storage
- JSON serialization of conversation state
- Automatic table creation on startup
- Session recovery across server restarts

### 📉 Context Management
- `MessageCountingChatReducer` limits context to 20 messages
- Prevents token overflow in long conversations
- Managed via `InMemoryChatHistoryProvider`

### 🔌 Clean DI Setup
```csharp
// Program.cs - Clean and declarative
builder.Services.AddCoreAiAgent(options =>
{
    options.Endpoint = "http://localhost:11434";
    options.Model = "gpt-oss:20b-cloud";
});

builder.Services.AddSqliteSessionStore("sessions.db");
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/chat` | Send message with custom userId |
| `POST` | `/api/chat/human` | Test endpoint (fixed session) |
| `GET` | `/api/chat/health` | Health check |

### Example Request
```bash
curl -X POST http://localhost:5057/api/chat/human \
  -H "Content-Type: application/json" \
  -d '{"message": "Hola, ¿quién eres?"}'
```

### Example Response
```json
{
  "response": "¡Hola! Soy HANNI, tu asistente personal."
}
```

## Project Structure

```
PolaperBotV2/
├── PolaperBot.Core.AI/                 # Core domain library
│   ├── Configuration/
│   │   ├── AgentInstructions.cs        # System prompts
│   │   └── AgentOptions.cs             # Configuration POCOs
│   ├── Extensions/
│   │   ├── AgentBuilderExtensions.cs   # Agent DI setup
│   │   ├── InstructionsExtensions.cs   # Instructions DI
│   │   └── ToolsExtensions.cs          # Tools DI
│   ├── Services/
│   │   └── AgentService.cs             # Agent orchestration
│   ├── Sessions/
│   │   └── ISessionStore.cs            # Session abstraction
│   └── Tools/
│       └── MemoryTool.cs               # Memory persistence tool
│
├── PolaperBot.Infra/                   # Infrastructure layer
│   ├── Extensions/
│   │   └── InfraExtensions.cs          # Infra DI setup
│   └── Sessions/
│       └── SqliteSessionStore.cs       # SQLite implementation
│
├── PolaperBot.Api/                     # API layer
│   ├── Endpoints/
│   │   └── ChatEndpoints.cs            # Endpoint definitions
│   ├── Program.cs                      # App entry point
│   └── appsettings.json                # Configuration
│
└── MEMORY.MD                           # Project documentation
```

## Design Patterns Used

| Pattern | Application |
|---------|-------------|
| **Dependency Injection** | All services registered via DI containers |
| **Repository Pattern** | `ISessionStore` abstracts data access |
| **Extension Methods** | Clean service registration per layer |
| **Options Pattern** | `OllamaOptions`, `AgentOptions` |
| **Factory Pattern** | Tool building via `BuildAgentTools()` |

## Getting Started

### Prerequisites
- .NET 10 SDK
- Ollama running locally with a model

### Run
```bash
# Clone and navigate
cd PolaperBotV2

# Run the API
dotnet run --project PolaperBot.Api
```

### Configuration (appsettings.json)
```json
{
  "Database": {
    "SqlitePath": "sessions.db"
  },
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "Model": "gpt-oss:20b-cloud"
  }
}
```

## Skills Demonstrated

- ✅ **Clean Architecture** - Separation of concerns across layers
- ✅ **Modern .NET** - .NET 10 with Minimal APIs
- ✅ **AI Integration** - Microsoft Agent Framework & Ollama
- ✅ **Persistence** - SQLite with async operations
- ✅ **DI Best Practices** - Extension methods for clean registration
- ✅ **Async/Await** - Non-blocking I/O throughout
- ✅ **JSON Serialization** - Session state persistence
- ✅ **Error Handling** - Graceful fallbacks for session deserialization

## License

MIT License

---

*Built with ❤️ using .NET 10 and Microsoft Agent Framework*
