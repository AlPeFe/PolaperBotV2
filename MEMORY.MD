# PolaperBotV2 - Project Planning

## Project Overview
Recreation of PolaperBot.Core.AI using the latest Microsoft Agent Framework tools and patterns.

## Tech Stack
- .NET 10 Class Library
- .NET 10 Minimal API
- SQLite (Session persistence)
- Microsoft.Agent.Framework 1.0.0-rc1
- OllamaSharp 5.4.16
- Google.Apis.Gmail.v1
- Google.Apis.Calendar.v3
- Ollama (gpt-oss:20b-cloud)

## Project Structure
```
PolaperBotV2/
├── PolaperBot.Core.AI/           # Core AI Library (.NET 10)
│   ├── Configuration/
│   │   ├── AgentInstructions.cs  # Agent system prompts
│   │   └── AgentOptions.cs       # Configuration POCOs (Ollama, Google, Database)
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs  # Main DI entry point
│   │   ├── AgentBuilderExtensions.cs       # Agent-specific DI
│   │   ├── ToolsExtensions.cs              # Tools DI
│   │   └── InstructionsExtensions.cs       # Instructions DI
│   ├── Services/
│   │   ├── AgentService.cs       # Agent orchestration
│   │   └── GoogleServicesFactory.cs  # Google OAuth factory
│   ├── Sessions/
│   │   └── ISessionStore.cs      # Session store interface
│   └── Tools/
│       ├── MemoryTool.cs         # MEMORY.MD tool
│       ├── GmailTool.cs          # Gmail integration
│       ├── GoogleCalendarTool.cs # Calendar integration
│       ├── BashTool.cs           # Shell commands
│       └── FileSystemTool.cs     # File operations
├── PolaperBot.Infra/            # Infrastructure Layer (.NET 10)
│   ├── Extensions/
│   │   └── InfraExtensions.cs   # DI extensions
│   └── Sessions/
│       └── SqliteSessionStore.cs # SQLite session persistence
├── PolaperBot.Api/              # Minimal API (.NET 10)
│   ├── credentials/             # Google credentials folder
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
| Core.AI | `Microsoft.Extensions.Configuration.Abstractions` | `10.0.3` |
| Core.AI | `Microsoft.Extensions.DependencyInjection.Abstractions` | `10.0.3` |
| Core.AI | `OllamaSharp` | `5.4.16` |
| Core.AI | `Google.Apis.Calendar.v3` | `1.69.0.3667` |
| Core.AI | `Google.Apis.Gmail.v1` | `1.69.0.3742` |
| Infra | `Microsoft.Agents.AI` | `1.0.0-rc1` |
| Infra | `Microsoft.Data.Sqlite` | `10.0.2` |
| Api | `Microsoft.AspNetCore.OpenApi` | `10.0.2` |

## AI Tools Available

### MemoryTool
- `SaveToMemory(data)` - Save important data to MEMORY.MD
- `ReadMemory()` - Read stored memory

### GmailTool
- `SendEmail(destinatario, asunto, contenido)` - Send email
- `SummarizeEmailsByDate(fecha)` - Get email summary by date

### GoogleCalendarTool
- `GetCurrentDateTime()` - Get current date/time context
- `CreateEvent(titulo, descripcion, fechaInicio, fechaFin, ubicacion)` - Create event
- `GetUpcomingEvents(maxResultados)` - List upcoming events
- `GetEventsByDate(fecha)` - Get events by date
- `DeleteEvent(eventoId)` - Delete event

### BashTool (Shell Access)
- `ExecuteCommand(command)` - Execute shell/bash commands
- `GetSystemInfo()` - Get OS, user, hostname, drives info

### FileSystemTool (Full Permissions)
- `ReadFile(filePath)` - Read file contents
- `WriteFile(filePath, content)` - Create/overwrite files
- `AppendToFile(filePath, content)` - Append to files
- `ListDirectory(directoryPath)` - List files and directories
- `CreateDirectory(directoryPath)` - Create directories
- `DeleteFile(filePath)` - Delete files
- `DeleteDirectory(directoryPath)` - Delete directories
- `CopyFile(sourcePath, destinationPath)` - Copy files
- `Move(sourcePath, destinationPath)` - Move/rename files
- `Exists(path)` - Check if file/directory exists
- `SearchFiles(pattern, directoryPath)` - Search files by pattern

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
| 9 | Add Gmail integration | ✅ |
| 10 | Add Google Calendar integration | ✅ |
| 11 | Create GoogleServicesFactory with OAuth | ✅ |
| 12 | Add BashTool for shell commands | ✅ |
| 13 | Add FileSystemTool for file operations | ✅ |
| 14 | Refactor tools to use proper DI | ✅ |
| 15 | Add Usage tracking for observability | ✅ |
| 16 | Add HeartbeatService with configurable triggers | ✅ |

## Key Design Decisions

1. **Layered Architecture**: Core.AI (abstractions) → Infra (implementations) → Api
2. **SQLite Session Persistence**: Sessions serialized to JSON and stored in SQLite
3. **Chat Reduction**: `InMemoryChatHistoryProvider` with 20-message limit
4. **DI Separation**: Each layer has its own extension methods
5. **Graceful Degradation**: Tools disabled when Google credentials missing
6. **Factory Pattern**: `GoogleServicesFactory` handles OAuth and service creation
7. **Full System Access**: BashTool and FileSystemTool with complete permissions
8. **Usage Tracking**: Processing time stored for observability (not retrieved on load)
9. **Heartbeat Service**: Background service with configurable triggers (GmailHbs, RemindersHbs)

## Heartbeat Service

Configurable background service that executes triggers at specified intervals:

```json
{
  "Heartbeat": {
    "Enabled": true,
    "IntervalMinutes": 5,
    "EnabledTriggers": ["GmailHbs", "RemindersHbs"]
  }
}
```

Available triggers:
- `GmailHbs` - Gmail heartbeat trigger (placeholder)
- `RemindersHbs` - Reminders heartbeat trigger (placeholder)

To add new triggers, implement `IHeartbeatTrigger` interface and register in DI.

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
  "Ollama": { 
    "Endpoint": "http://localhost:11434", 
    "Model": "gpt-oss:20b-cloud" 
  },
  "Google": {
    "CredentialsPath": "./credentials/google_credentials.json",
    "TokenFolder": "./credentials/google_token",
    "EnableGmail": true,
    "EnableCalendar": true
  },
  "Agent": { "Name": "HanniAssistant", "MemoryPath": "./MEMORY.MD" }
}
```

## Session Persistence

Sessions are stored in SQLite with this schema:
```sql
CREATE TABLE Sessions (
    UserId INTEGER PRIMARY KEY,
    SessionJson TEXT NOT NULL,
    UsageJson TEXT,
    UpdatedAt TEXT NOT NULL
)
```

- `LoadOrCreateAsync(userId)`: Loads session from DB or creates new
- `SaveAsync(userId, session, usage)`: Serializes and saves session with usage metrics

## Session Usage Tracking

Usage is stored for observability/metrics (not retrieved when loading sessions):
```csharp
public class SessionUsage
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public string? Model { get; set; }
    public double ProcessingTimeMs { get; set; }
    public int ToolCalls { get; set; }
    public DateTime Timestamp { get; set; }
}
```

Usage is captured automatically after each message and stored in `UsageJson` column.

## Google Integration Setup

1. Create Google Cloud project
2. Enable Gmail API and Calendar API
3. Create OAuth 2.0 credentials (Desktop app)
4. Download JSON to `credentials/google_credentials.json`
5. First run triggers OAuth authorization flow

---
*Last updated: 2026-02-25*
