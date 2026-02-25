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
│  • MemoryTool         • GmailTool                           │
│  • GoogleCalendarTool • AgentInstructions                   │
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
| **Integrations** | Gmail API, Google Calendar API |

## Key Features

### 🧠 AI Agent with Tools
- **MemoryTool** - Persistent memory storage in MEMORY.MD
- **GmailTool** - Send emails, summarize emails by date
- **GoogleCalendarTool** - Create, update, delete, and list calendar events
- Extensible tool system via `AITool[]` registration
- Natural language processing in Spanish

### 📧 Gmail Integration
- `SendEmail` - Send emails through Gmail
- `SummarizeEmailsByDate` - Get email summaries for a specific date

### 📅 Google Calendar Integration
- `CreateEvent` - Create new calendar events
- `GetUpcomingEvents` - List upcoming events
- `GetEventsByDate` - Get events for a specific date
- `DeleteEvent` - Delete calendar events by ID

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
builder.Services.AddCoreAiAgent(
    ollama => 
    {
        ollama.Endpoint = "http://localhost:11434";
        ollama.Model = "gpt-oss:20b-cloud";
    },
    google => 
    {
        google.CredentialsPath = "./credentials/google_credentials.json";
        google.TokenFolder = "./credentials/google_token";
        google.EnableGmail = true;
        google.EnableCalendar = true;
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
│   │   ├── AgentService.cs             # Agent orchestration
│   │   └── GoogleServicesFactory.cs    # Google OAuth factory
│   ├── Sessions/
│   │   └── ISessionStore.cs            # Session abstraction
│   └── Tools/
│       ├── MemoryTool.cs               # Memory persistence tool
│       ├── GmailTool.cs                # Gmail integration
│       └── GoogleCalendarTool.cs       # Calendar integration
│
├── PolaperBot.Infra/                   # Infrastructure layer
│   ├── Extensions/
│   │   └── InfraExtensions.cs          # Infra DI setup
│   └── Sessions/
│       └── SqliteSessionStore.cs       # SQLite implementation
│
├── PolaperBot.Api/                     # API layer
│   ├── credentials/                    # Google credentials folder
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
| **Options Pattern** | `OllamaOptions`, `GoogleOptions`, `AgentOptions` |
| **Factory Pattern** | `GoogleServicesFactory` for OAuth, `BuildAgentTools()` for tools |
| **Graceful Degradation** | Tools disabled when credentials missing |

## Getting Started

### Prerequisites
- .NET 10 SDK
- Ollama running locally with a model
- (Optional) Google Cloud credentials for Gmail/Calendar

### Run
```bash
# Clone and navigate
cd PolaperBotV2

# Run the API
dotnet run --project PolaperBot.Api
```

### Google Integration Setup (Optional)

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a project and enable Gmail API and Calendar API
3. Create OAuth 2.0 credentials (Desktop app)
4. Download credentials JSON
5. Save to `PolaperBot.Api/credentials/google_credentials.json`
6. First run will open browser for OAuth authorization

### Configuration (appsettings.json)
```json
{
  "Database": {
    "SqlitePath": "sessions.db"
  },
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "Model": "gpt-oss:20b-cloud"
  },
  "Google": {
    "CredentialsPath": "./credentials/google_credentials.json",
    "TokenFolder": "./credentials/google_token",
    "EnableGmail": true,
    "EnableCalendar": true
  }
}
```

## Skills Demonstrated

- ✅ **Clean Architecture** - Separation of concerns across layers
- ✅ **Modern .NET** - .NET 10 with Minimal APIs
- ✅ **AI Integration** - Microsoft Agent Framework & Ollama
- ✅ **External APIs** - Gmail API & Google Calendar API integration
- ✅ **OAuth 2.0** - Google authentication flow
- ✅ **Persistence** - SQLite with async operations
- ✅ **DI Best Practices** - Extension methods for clean registration
- ✅ **Async/Await** - Non-blocking I/O throughout
- ✅ **JSON Serialization** - Session state persistence
- ✅ **Error Handling** - Graceful fallbacks for missing credentials
- ✅ **Tool System** - Extensible AI function calling

## License

MIT License

---

*Built with ❤️ using .NET 10 and Microsoft Agent Framework*
