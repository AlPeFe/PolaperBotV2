using PolaperBot.Api.Endpoints;
using PolaperBot.Core.AI;
using PolaperBot.Core.AI.Configuration;
using PolaperBot.Infra;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var ollamaOptions = new OllamaOptions
{
    Endpoint = builder.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434",
    Model = builder.Configuration["Ollama:Model"] ?? "gpt-oss:20b-cloud"
};

var dbPath = builder.Configuration["Database:SqlitePath"] ?? "sessions.db";

builder.Services.AddCoreAiAgent(options =>
{
    options.Endpoint = ollamaOptions.Endpoint;
    options.Model = ollamaOptions.Model;
});

builder.Services.AddSqliteSessionStore(dbPath);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapChatEndpoints();

app.Run();
