using PolaperBot.Api.Endpoints;
using PolaperBot.Core.AI;
using PolaperBot.Core.AI.Configuration;
using PolaperBot.Core.AI.Extensions;
using PolaperBot.Infra;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenApi();

var ollamaOptions = new OllamaOptions
{
    Endpoint = builder.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434",
    Model = builder.Configuration["Ollama:Model"] ?? "gpt-oss:20b-cloud"
};

var googleOptions = new GoogleOptions
{
    CredentialsPath = builder.Configuration["Google:CredentialsPath"] ?? "./credentials/google_credentials.json",
    TokenFolder = builder.Configuration["Google:TokenFolder"] ?? "./credentials/google_token",
    EnableGmail = bool.TryParse(builder.Configuration["Google:EnableGmail"], out var enableGmail) && enableGmail,
    EnableCalendar = bool.TryParse(builder.Configuration["Google:EnableCalendar"], out var enableCalendar) && enableCalendar
};

var telegramOptions = new TelegramOptions
{
    BotToken = builder.Configuration["Telegram:BotToken"] ?? string.Empty,
    MainChatId = long.TryParse(builder.Configuration["Telegram:MainChatId"], out var chatId) ? chatId : 0
};

var sessionDbPath = builder.Configuration["Database:SqlitePath"] ?? "sessions.db";
var remindersDbPath = builder.Configuration["Database:RemindersDbPath"] ?? "reminders.db";

builder.Services.AddCoreAiAgent(
    options =>
    {
        options.Endpoint = ollamaOptions.Endpoint;
        options.Model = ollamaOptions.Model;
    },
    options =>
    {
        options.CredentialsPath = googleOptions.CredentialsPath;
        options.TokenFolder = googleOptions.TokenFolder;
        options.EnableGmail = googleOptions.EnableGmail;
        options.EnableCalendar = googleOptions.EnableCalendar;
    },
    options =>
    {
        options.BotToken = telegramOptions.BotToken;
        options.MainChatId = telegramOptions.MainChatId;
    });

builder.Services.AddSqliteSessionStore(sessionDbPath);
builder.Services.AddSqliteReminderRepository(remindersDbPath);
builder.Services.AddHeartbeatService();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapChatEndpoints();

if (!string.IsNullOrEmpty(telegramOptions.BotToken))
{
    app.MapTelegramEndpoints();
}

try
{
    Log.Information("Starting PolaperBot API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
