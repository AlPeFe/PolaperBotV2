# AGENTS.md

## Code style
Clean code
Separation of concepts
simple no overengineer
use DI
usa como refrencia el proyecto PolaperBot en la carpeta superior a esta


## task
Basado en el proyecto actual, en la libreria PolaperBot.Core.AI quiero recrearla con nuevas tools que han aparecido para el mismo FrameWork ( microsoft agent framework)

## tech stack
.net10 class library
.net 10 minimal API
SqlLite
Microsoft.Agent.Framework 
usa la ultima version disponible de las librerias, en el proyecto actual estan desactualiuzadas. 
añade en appsettings las conexiones a bbdd (usa sqlite, o sea que solo el path)

## tasks

Crea una class library con el CORE para la IA, extenion para DI del agente de IA, usar OLLAMA como provider, recuerda que la implementación actual puede ser incorrecta, no asumas que el codigo actual es bueno.

Una vez creado esto añade una api minima muy simple que reciba un texto y el agente pueda responder, este endpoint se va a usar por ahora para seguir desarrollando y testear facilmente, se movera a otra cosa en produccion.

Implement InMemory conversation sessions for a Telegram bot using Microsoft Agent Framework (.NET 10).

Requirements:
- SessionStore as singleton using ConcurrentDictionary<long, AgentSession> (key = Telegram userId)
- AgentSession created via agent.CreateSessionAsync()
- MessageCountingChatReducer limited to 20 messages

Crea una tool de aent framework de prueba, una tool que pueda almacenar en un fichero MD llamado MEMORY.MD datos que se consideren muy impoortantes

Recuerda separar en el DI del agente muy bien cada parte, las instruction por un lado, las tools por otras todo como extensiones.
