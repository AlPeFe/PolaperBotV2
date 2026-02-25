namespace PolaperBot.Core.AI.Configuration;

public class AgentOptions
{
    public string Name { get; set; } = "HanniAssistant";
    public string Description { get; set; } = "Asistente conversacional natural";
    public string MemoryPath { get; set; } = "./MEMORY.MD";
}

public class OllamaOptions
{
    public string Endpoint { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "gpt-oss:20b-cloud";
}

public class DatabaseOptions
{
    public string SqlitePath { get; set; } = "Data Source=polaperbot.db";
}

public class GoogleOptions
{
    public string CredentialsPath { get; set; } = "./credentials/google_credentials.json";
    public string TokenFolder { get; set; } = "./credentials/google_token";
    public bool EnableGmail { get; set; } = true;
    public bool EnableCalendar { get; set; } = true;
}
