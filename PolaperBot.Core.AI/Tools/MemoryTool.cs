using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace PolaperBot.Core.AI.Tools;

public interface IMemoryService
{
    string ReadMemory();
    void SaveToMemory(string data);
}

public class MemoryService : IMemoryService
{
    private readonly string _memoryPath;

    public MemoryService(string memoryPath)
    {
        _memoryPath = memoryPath;
        EnsureMemoryFileExists();
    }

    private void EnsureMemoryFileExists()
    {
        var directory = Path.GetDirectoryName(_memoryPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(_memoryPath))
        {
            File.WriteAllText(_memoryPath, "# Memoria de HANNI\n\nDatos importantes del usuario.\n\n");
        }
    }

    public string ReadMemory()
    {
        return File.Exists(_memoryPath) 
            ? File.ReadAllText(_memoryPath) 
            : "No hay memoria almacenada.";
    }

    public void SaveToMemory(string data)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        var entry = $"\n## [{timestamp}]\n{data}\n";
        File.AppendAllText(_memoryPath, entry);
    }
}

public class MemoryTool
{
    private readonly IMemoryService _memoryService;

    public MemoryTool(IMemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    [Description("Guarda información importante en la memoria permanente. Usa esto cuando el usuario te diga algo que debas recordar para futuras conversaciones.")]
    public string SaveToMemory(
        [Description("El dato o información importante a recordar")] string data)
    {
        _memoryService.SaveToMemory(data);
        return "✓ Información guardada en memoria permanente";
    }

    [Description("Lee la memoria permanente para recordar información importante del usuario")]
    public string ReadMemory()
    {
        return _memoryService.ReadMemory();
    }

    public IEnumerable<AITool> AsAITools()
    {
        yield return AIFunctionFactory.Create(SaveToMemory);
        yield return AIFunctionFactory.Create(ReadMemory);
    }
}
