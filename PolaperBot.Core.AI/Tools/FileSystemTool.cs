using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace PolaperBot.Core.AI.Tools;

public interface IFileSystemTool
{
    IEnumerable<AITool> AsAITools();
}

public class FileSystemTool : IFileSystemTool
{
    private readonly ILogger<FileSystemTool> _logger;

    public FileSystemTool(ILogger<FileSystemTool> logger)
    {
        _logger = logger;
    }

    [Description("Lee el contenido completo de un archivo de texto.")]
    public async Task<string> ReadFile(
        [Description("Ruta del archivo a leer")] string filePath)
    {
        try
        {
            var fullPath = GetFullPath(filePath);

            if (!File.Exists(fullPath))
            {
                return ErrorResult("El archivo no existe", filePath, fullPath);
            }

            var content = await File.ReadAllTextAsync(fullPath);
            var fileInfo = new FileInfo(fullPath);

            _logger.LogInformation("File read: {Path}", fullPath);

            return JsonSerializer.Serialize(new
            {
                success = true,
                path = fullPath,
                content,
                sizeBytes = fileInfo.Length,
                lastModified = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read file: {Path}", filePath);
            return ErrorResult(ex.Message, filePath);
        }
    }

    [Description("Escribe contenido en un archivo. Si el archivo existe, se sobrescribe. Los directorios padre se crean automáticamente.")]
    public async Task<string> WriteFile(
        [Description("Ruta del archivo")] string filePath,
        [Description("Contenido a escribir en el archivo")] string content)
    {
        try
        {
            var fullPath = GetFullPath(filePath);
            EnsureParentDirectoryExists(fullPath);

            await File.WriteAllTextAsync(fullPath, content);
            var fileInfo = new FileInfo(fullPath);

            _logger.LogInformation("File written: {Path} ({Size} bytes)", fullPath, fileInfo.Length);

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = "Archivo escrito correctamente",
                path = fullPath,
                sizeBytes = fileInfo.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write file: {Path}", filePath);
            return ErrorResult(ex.Message, filePath);
        }
    }

    [Description("Añade contenido al final de un archivo existente. Si no existe, lo crea.")]
    public async Task<string> AppendToFile(
        [Description("Ruta del archivo")] string filePath,
        [Description("Contenido a añadir")] string content)
    {
        try
        {
            var fullPath = GetFullPath(filePath);
            EnsureParentDirectoryExists(fullPath);

            await File.AppendAllTextAsync(fullPath, content);
            var fileInfo = new FileInfo(fullPath);

            _logger.LogInformation("Content appended to: {Path}", fullPath);

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = "Contenido añadido correctamente",
                path = fullPath,
                sizeBytes = fileInfo.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append to file: {Path}", filePath);
            return ErrorResult(ex.Message, filePath);
        }
    }

    [Description("Lista todos los archivos y directorios en una ruta. Usa '.' para el directorio actual.")]
    public Task<string> ListDirectory(
        [Description("Ruta del directorio a listar (usa '.' para directorio actual)")] string directoryPath)
    {
        try
        {
            var fullPath = GetFullPath(directoryPath);

            if (!Directory.Exists(fullPath))
            {
                return Task.FromResult(ErrorResult("El directorio no existe", directoryPath, fullPath));
            }

            var files = Directory.GetFiles(fullPath, "*", SearchOption.TopDirectoryOnly)
                .Select(f => new FileInfo(f))
                .Select(fi => new
                {
                    type = "file",
                    name = fi.Name,
                    extension = fi.Extension,
                    sizeBytes = fi.Length,
                    lastModified = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                });

            var directories = Directory.GetDirectories(fullPath, "*", SearchOption.TopDirectoryOnly)
                .Select(d => new DirectoryInfo(d))
                .Select(di => new
                {
                    type = "directory",
                    name = di.Name,
                    lastModified = di.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                });

            var items = files.Cast<object>().Concat(directories).ToList();

            _logger.LogInformation("Directory listed: {Path} ({Count} items)", fullPath, items.Count);

            return Task.FromResult(JsonSerializer.Serialize(new
            {
                success = true,
                path = fullPath,
                totalItems = items.Count,
                files = items.Count(i => ((dynamic)i).type == "file"),
                directories = items.Count(i => ((dynamic)i).type == "directory"),
                items
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list directory: {Path}", directoryPath);
            return Task.FromResult(ErrorResult(ex.Message, directoryPath));
        }
    }

    [Description("Crea un nuevo directorio. Los directorios padre se crean automáticamente.")]
    public Task<string> CreateDirectory(
        [Description("Ruta del directorio a crear")] string directoryPath)
    {
        try
        {
            var fullPath = GetFullPath(directoryPath);

            if (Directory.Exists(fullPath))
            {
                return Task.FromResult(JsonSerializer.Serialize(new
                {
                    success = true,
                    message = "El directorio ya existe",
                    path = fullPath
                }));
            }

            Directory.CreateDirectory(fullPath);

            _logger.LogInformation("Directory created: {Path}", fullPath);

            return Task.FromResult(JsonSerializer.Serialize(new
            {
                success = true,
                message = "Directorio creado correctamente",
                path = fullPath
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create directory: {Path}", directoryPath);
            return Task.FromResult(ErrorResult(ex.Message, directoryPath));
        }
    }

    [Description("Elimina un archivo. ADVERTENCIA: Esta acción es irreversible.")]
    public Task<string> DeleteFile(
        [Description("Ruta del archivo a eliminar")] string filePath)
    {
        try
        {
            var fullPath = GetFullPath(filePath);

            if (!File.Exists(fullPath))
            {
                return Task.FromResult(ErrorResult("El archivo no existe", filePath, fullPath));
            }

            File.Delete(fullPath);

            _logger.LogWarning("File deleted: {Path}", fullPath);

            return Task.FromResult(JsonSerializer.Serialize(new
            {
                success = true,
                message = "Archivo eliminado correctamente",
                path = fullPath
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {Path}", filePath);
            return Task.FromResult(ErrorResult(ex.Message, filePath));
        }
    }

    [Description("Elimina un directorio y todo su contenido. ADVERTENCIA: Esta acción es irreversible.")]
    public Task<string> DeleteDirectory(
        [Description("Ruta del directorio a eliminar")] string directoryPath)
    {
        try
        {
            var fullPath = GetFullPath(directoryPath);

            if (!Directory.Exists(fullPath))
            {
                return Task.FromResult(ErrorResult("El directorio no existe", directoryPath, fullPath));
            }

            Directory.Delete(fullPath, recursive: true);

            _logger.LogWarning("Directory deleted: {Path}", fullPath);

            return Task.FromResult(JsonSerializer.Serialize(new
            {
                success = true,
                message = "Directorio eliminado correctamente con todo su contenido",
                path = fullPath
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete directory: {Path}", directoryPath);
            return Task.FromResult(ErrorResult(ex.Message, directoryPath));
        }
    }

    [Description("Copia un archivo de una ubicación a otra.")]
    public Task<string> CopyFile(
        [Description("Ruta del archivo origen")] string sourcePath,
        [Description("Ruta de destino")] string destinationPath)
    {
        try
        {
            var fullSource = GetFullPath(sourcePath);
            var fullDest = GetFullPath(destinationPath);

            if (!File.Exists(fullSource))
            {
                return Task.FromResult(ErrorResult("El archivo origen no existe", sourcePath, fullSource));
            }

            EnsureParentDirectoryExists(fullDest);
            File.Copy(fullSource, fullDest, overwrite: true);

            _logger.LogInformation("File copied: {Source} -> {Dest}", fullSource, fullDest);

            return Task.FromResult(JsonSerializer.Serialize(new
            {
                success = true,
                message = "Archivo copiado correctamente",
                sourcePath = fullSource,
                destinationPath = fullDest
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy file: {Source} -> {Dest}", sourcePath, destinationPath);
            return Task.FromResult(ErrorResult(ex.Message, sourcePath));
        }
    }

    [Description("Mueve o renombra un archivo o directorio.")]
    public Task<string> Move(
        [Description("Ruta origen")] string sourcePath,
        [Description("Ruta destino")] string destinationPath)
    {
        try
        {
            var fullSource = GetFullPath(sourcePath);
            var fullDest = GetFullPath(destinationPath);

            if (File.Exists(fullSource))
            {
                EnsureParentDirectoryExists(fullDest);
                if (File.Exists(fullDest))
                    File.Delete(fullDest);
                File.Move(fullSource, fullDest);

                _logger.LogInformation("File moved: {Source} -> {Dest}", fullSource, fullDest);
            }
            else if (Directory.Exists(fullSource))
            {
                EnsureParentDirectoryExists(fullDest);
                if (Directory.Exists(fullDest))
                    Directory.Delete(fullDest, true);
                Directory.Move(fullSource, fullDest);

                _logger.LogInformation("Directory moved: {Source} -> {Dest}", fullSource, fullDest);
            }
            else
            {
                return Task.FromResult(ErrorResult("El archivo o directorio no existe", sourcePath, fullSource));
            }

            return Task.FromResult(JsonSerializer.Serialize(new
            {
                success = true,
                message = "Movido/renombrado correctamente",
                sourcePath = fullSource,
                destinationPath = fullDest
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move: {Source} -> {Dest}", sourcePath, destinationPath);
            return Task.FromResult(ErrorResult(ex.Message, sourcePath));
        }
    }

    [Description("Verifica si existe un archivo o directorio.")]
    public Task<string> Exists(
        [Description("Ruta a verificar")] string path)
    {
        try
        {
            var fullPath = GetFullPath(path);
            var isFile = File.Exists(fullPath);
            var isDirectory = Directory.Exists(fullPath);

            return Task.FromResult(JsonSerializer.Serialize(new
            {
                success = true,
                path = fullPath,
                exists = isFile || isDirectory,
                type = isFile ? "file" : isDirectory ? "directory" : "not_found"
            }));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ErrorResult(ex.Message, path));
        }
    }

    [Description("Busca archivos que coincidan con un patrón en un directorio.")]
    public Task<string> SearchFiles(
        [Description("Patrón de búsqueda (ejemplo: *.txt, test*.cs)")] string pattern,
        [Description("Directorio donde buscar")] string directoryPath)
    {
        try
        {
            var fullPath = GetFullPath(directoryPath);

            if (!Directory.Exists(fullPath))
            {
                return Task.FromResult(ErrorResult("El directorio no existe", directoryPath, fullPath));
            }

            var files = Directory.GetFiles(fullPath, pattern, SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
                .Select(fi => new
                {
                    path = fi.FullName,
                    name = fi.Name,
                    sizeBytes = fi.Length,
                    lastModified = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            _logger.LogInformation("Search completed: {Pattern} in {Path} ({Count} results)", pattern, fullPath, files.Count);

            return Task.FromResult(JsonSerializer.Serialize(new
            {
                success = true,
                pattern,
                searchDirectory = fullPath,
                totalResults = files.Count,
                files
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search files: {Pattern}", pattern);
            return Task.FromResult(ErrorResult(ex.Message, directoryPath));
        }
    }

    public IEnumerable<AITool> AsAITools()
    {
        yield return AIFunctionFactory.Create(ReadFile);
        yield return AIFunctionFactory.Create(WriteFile);
        yield return AIFunctionFactory.Create(AppendToFile);
        yield return AIFunctionFactory.Create(ListDirectory);
        yield return AIFunctionFactory.Create(CreateDirectory);
        yield return AIFunctionFactory.Create(DeleteFile);
        yield return AIFunctionFactory.Create(DeleteDirectory);
        yield return AIFunctionFactory.Create(CopyFile);
        yield return AIFunctionFactory.Create(Move);
        yield return AIFunctionFactory.Create(Exists);
        yield return AIFunctionFactory.Create(SearchFiles);
    }

    private static string GetFullPath(string path)
    {
        return Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
    }

    private static void EnsureParentDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static string ErrorResult(string errorMessage, string requestedPath, string? fullPath = null)
    {
        return JsonSerializer.Serialize(new
        {
            success = false,
            error = errorMessage,
            requestedPath,
            fullPath
        });
    }
}
