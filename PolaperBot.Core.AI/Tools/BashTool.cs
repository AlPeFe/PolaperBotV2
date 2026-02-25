using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace PolaperBot.Core.AI.Tools;

public interface IBashTool
{
    IEnumerable<AITool> AsAITools();
}

public class BashTool : IBashTool
{
    private readonly ILogger<BashTool> _logger;
    private readonly bool _isWindows;

    public BashTool(ILogger<BashTool> logger)
    {
        _logger = logger;
        _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    [Description("Ejecuta un comando en la terminal/shell. En Linux/Mac usa bash, en Windows usa PowerShell. Tiene permisos completos del sistema.")]
    public async Task<string> ExecuteCommand(
        [Description("El comando a ejecutar")] string command)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            if (_isWindows)
            {
                processStartInfo.FileName = "powershell.exe";
                processStartInfo.Arguments = $"-NoProfile -Command \"{command.Replace("\"", "\\\"")}\"";
            }
            else
            {
                processStartInfo.FileName = "/bin/bash";
                processStartInfo.Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"";
            }

            using var process = new Process { StartInfo = processStartInfo };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                    errorBuilder.AppendLine(e.Data);
            };

            var startTime = DateTime.Now;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            await process.WaitForExitAsync(cts.Token);

            var executionTime = (DateTime.Now - startTime).TotalSeconds;

            _logger.LogInformation("Command executed: {Command} (ExitCode: {ExitCode})", 
                command[..Math.Min(command.Length, 50)], process.ExitCode);

            return JsonSerializer.Serialize(new
            {
                success = process.ExitCode == 0,
                exitCode = process.ExitCode,
                stdout = outputBuilder.ToString().TrimEnd(),
                stderr = errorBuilder.ToString().TrimEnd(),
                command,
                workingDirectory = processStartInfo.WorkingDirectory,
                executionTimeSeconds = Math.Round(executionTime, 2)
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Command timed out: {Command}", command);
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = "El comando excedió el tiempo límite de 60 segundos",
                command
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute command: {Command}", command);
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = ex.Message,
                exceptionType = ex.GetType().Name,
                command
            });
        }
    }

    [Description("Obtiene información del sistema: OS, usuario, hostname, directorio actual, discos, etc.")]
    public Task<string> GetSystemInfo()
    {
        var result = new
        {
            os = Environment.OSVersion.ToString(),
            platform = RuntimeInformation.OSDescription,
            architecture = RuntimeInformation.OSArchitecture.ToString(),
            machineName = Environment.MachineName,
            userName = Environment.UserName,
            currentDirectory = Directory.GetCurrentDirectory(),
            is64BitOS = Environment.Is64BitOperatingSystem,
            processorCount = Environment.ProcessorCount,
            dotnetVersion = Environment.Version.ToString(),
            systemDirectory = Environment.SystemDirectory,
            homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            tempPath = Path.GetTempPath(),
            drives = DriveInfo.GetDrives().Select(d => new
            {
                name = d.Name,
                type = d.DriveType.ToString(),
                totalGB = d.IsReady ? Math.Round(d.TotalSize / 1024.0 / 1024 / 1024, 2) : 0,
                freeGB = d.IsReady ? Math.Round(d.AvailableFreeSpace / 1024.0 / 1024 / 1024, 2) : 0
            })
        };

        return Task.FromResult(JsonSerializer.Serialize(result));
    }

    public IEnumerable<AITool> AsAITools()
    {
        yield return AIFunctionFactory.Create(ExecuteCommand);
        yield return AIFunctionFactory.Create(GetSystemInfo);
    }
}
