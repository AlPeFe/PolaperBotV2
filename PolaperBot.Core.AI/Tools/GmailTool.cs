using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace PolaperBot.Core.AI.Tools;

public interface IGmailTool
{
    IEnumerable<AITool> AsAITools();
}

public class GmailTool : IGmailTool
{
    private readonly GmailService? _gmailService;
    private readonly ILogger<GmailTool> _logger;

    public GmailTool(GmailService? gmailService, ILogger<GmailTool> logger)
    {
        _gmailService = gmailService;
        _logger = logger;

        if (_gmailService == null)
            _logger.LogWarning("GmailService not available - Gmail tools disabled");
    }

    [Description("Crea y envía un correo electrónico a través de Gmail")]
    public async Task<string> SendEmail(
        [Description("Dirección de correo del destinatario")] string destinatario = "",
        [Description("Asunto del correo")] string asunto = "",
        [Description("Contenido/cuerpo del correo")] string contenido = "")
    {
        if (_gmailService == null)
            return JsonSerializer.Serialize(new { success = false, error = "Gmail no está configurado" });

        if (string.IsNullOrWhiteSpace(destinatario))
            return JsonSerializer.Serialize(new { success = false, error = "El destinatario es requerido" });

        if (string.IsNullOrWhiteSpace(asunto))
            return JsonSerializer.Serialize(new { success = false, error = "El asunto es requerido" });

        if (string.IsNullOrWhiteSpace(contenido))
            return JsonSerializer.Serialize(new { success = false, error = "El contenido del correo es requerido" });

        try
        {
            var message = new Message
            {
                Raw = CreateBase64EncodedEmail(destinatario, asunto, contenido)
            };

            var request = _gmailService.Users.Messages.Send(message, "me");
            await request.ExecuteAsync();

            _logger.LogInformation("Email sent to {Recipient}", destinatario);

            return JsonSerializer.Serialize(new
            {
                success = true,
                mensaje = "Correo enviado exitosamente",
                correo = new
                {
                    destinatario,
                    asunto,
                    fechaEnvio = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
            return JsonSerializer.Serialize(new { success = false, error = $"Error al enviar correo: {ex.Message}" });
        }
    }

    [Description("Resume los correos recibidos en una fecha específica en formato yyyy-MM-dd")]
    public async Task<string> SummarizeEmailsByDate(
        [Description("Fecha para buscar correos en formato yyyy-MM-dd (ejemplo: 2026-02-11)")] string fecha = "")
    {
        if (_gmailService == null)
            return JsonSerializer.Serialize(new { success = false, error = "Gmail no está configurado" });

        if (string.IsNullOrWhiteSpace(fecha))
            return JsonSerializer.Serialize(new { success = false, error = "La fecha es requerida en formato yyyy-MM-dd" });

        if (!DateTime.TryParse(fecha, out var targetDate))
            return JsonSerializer.Serialize(new { success = false, error = "Formato de fecha inválido. Use yyyy-MM-dd" });

        try
        {
            var query = $"in:inbox after:{targetDate:yyyy/MM/dd} before:{targetDate.AddDays(1):yyyy/MM/dd}";

            var request = _gmailService.Users.Messages.List("me");
            request.Q = query;
            request.MaxResults = 50;

            var result = await request.ExecuteAsync();

            if (result.Messages == null || result.Messages.Count == 0)
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    fecha,
                    mensaje = $"No se encontraron correos recibidos en {fecha}",
                    totalCorreos = 0,
                    correos = Array.Empty<object>()
                });
            }

            var emailList = new List<object>();
            foreach (var messageItem in result.Messages.Take(20))
            {
                var messageRequest = _gmailService.Users.Messages.Get("me", messageItem.Id);
                messageRequest.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Metadata;
                messageRequest.MetadataHeaders = "From";
                messageRequest.MetadataHeaders = "Subject";
                messageRequest.MetadataHeaders = "Date";

                var message = await messageRequest.ExecuteAsync();

                var from = message.Payload?.Headers?.FirstOrDefault(h => h.Name == "From")?.Value ?? "Desconocido";
                var subject = message.Payload?.Headers?.FirstOrDefault(h => h.Name == "Subject")?.Value ?? "Sin asunto";
                var dateStr = message.Payload?.Headers?.FirstOrDefault(h => h.Name == "Date")?.Value ?? "";

                emailList.Add(new { remitente = from, asunto = subject, fecha = dateStr });
            }

            return JsonSerializer.Serialize(new
            {
                success = true,
                fecha,
                totalCorreos = result.Messages.Count,
                correosEnResumen = emailList.Count,
                correos = emailList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to summarize emails");
            return JsonSerializer.Serialize(new { success = false, error = $"Error al resumir correos: {ex.Message}" });
        }
    }

    private static string CreateBase64EncodedEmail(string to, string subject, string body)
    {
        var message = new StringBuilder();
        message.AppendLine($"To: {to}");
        message.AppendLine($"Subject: {subject}");
        message.AppendLine("Content-Type: text/plain; charset=utf-8");
        message.AppendLine();
        message.AppendLine(body);

        var bytes = Encoding.UTF8.GetBytes(message.ToString());
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }

    public IEnumerable<AITool> AsAITools()
    {
        if (_gmailService == null)
            yield break;

        yield return AIFunctionFactory.Create(SendEmail);
        yield return AIFunctionFactory.Create(SummarizeEmailsByDate);
    }
}
