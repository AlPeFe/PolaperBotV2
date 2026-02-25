using System.ComponentModel;
using System.Text.Json;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace PolaperBot.Core.AI.Tools;

public interface ICalendarTool
{
    IEnumerable<AITool> AsAITools();
}

public class GoogleCalendarTool : ICalendarTool
{
    private readonly CalendarService? _calendarService;
    private readonly ILogger<GoogleCalendarTool> _logger;

    public GoogleCalendarTool(CalendarService? calendarService, ILogger<GoogleCalendarTool> logger)
    {
        _calendarService = calendarService;
        _logger = logger;

        if (_calendarService == null)
            _logger.LogWarning("CalendarService not available - Calendar tools disabled");
    }

    [Description("Obtiene la fecha y hora actual. USA ESTA FUNCIÓN SIEMPRE antes de crear eventos para saber qué día es hoy y calcular fechas relativas como 'mañana', 'dentro de 3 días', etc.")]
    public Task<string> GetCurrentDateTime()
    {
        var now = DateTime.Now;
        var result = new
        {
            fechaActual = now.ToString("yyyy-MM-dd"),
            horaActual = now.ToString("HH:mm:ss"),
            diaSemana = now.DayOfWeek switch
            {
                DayOfWeek.Monday => "lunes",
                DayOfWeek.Tuesday => "martes",
                DayOfWeek.Wednesday => "miércoles",
                DayOfWeek.Thursday => "jueves",
                DayOfWeek.Friday => "viernes",
                DayOfWeek.Saturday => "sábado",
                DayOfWeek.Sunday => "domingo",
                _ => "desconocido"
            },
            mes = now.ToString("MMMM"),
            año = now.Year,
            zonaHoraria = "Europe/Madrid",
            contexto = $"Hoy es {now:dddd dd 'de' MMMM 'de' yyyy}, son las {now:HH:mm}. Fecha ISO: {now:yyyy-MM-dd}"
        };

        return Task.FromResult(JsonSerializer.Serialize(result));
    }

    [Description("Crea un nuevo evento en Google Calendar con título, fecha y hora de inicio y fin. PRIMERO usa GetCurrentDateTime para saber qué día es hoy.")]
    public async Task<string> CreateEvent(
        [Description("Título o resumen del evento")] string titulo = "",
        [Description("Descripción detallada del evento (opcional)")] string descripcion = "",
        [Description("Fecha y hora de inicio en formato yyyy-MM-dd HH:mm")] string fechaInicio = "",
        [Description("Fecha y hora de fin en formato yyyy-MM-dd HH:mm")] string fechaFin = "",
        [Description("Ubicación del evento (opcional)")] string ubicacion = "")
    {
        if (_calendarService == null)
            return JsonSerializer.Serialize(new { success = false, error = "Calendar no está configurado" });

        if (string.IsNullOrWhiteSpace(titulo))
            return JsonSerializer.Serialize(new { success = false, error = "El título del evento es requerido" });

        if (!DateTime.TryParse(fechaInicio, out var startDateTime))
            return JsonSerializer.Serialize(new { success = false, error = "Formato de fecha de inicio inválido. Use yyyy-MM-dd HH:mm" });

        if (!DateTime.TryParse(fechaFin, out var endDateTime))
            return JsonSerializer.Serialize(new { success = false, error = "Formato de fecha de fin inválido. Use yyyy-MM-dd HH:mm" });

        if (endDateTime <= startDateTime)
            return JsonSerializer.Serialize(new { success = false, error = "La fecha de fin debe ser posterior a la fecha de inicio" });

        try
        {
            var newEvent = new Event
            {
                Summary = titulo,
                Description = descripcion,
                Location = ubicacion,
                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = startDateTime,
                    TimeZone = "Europe/Madrid"
                },
                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = endDateTime,
                    TimeZone = "Europe/Madrid"
                },
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
                    {
                        new() { Method = "popup", Minutes = 30 },
                        new() { Method = "email", Minutes = 1440 }
                    }
                }
            };

            var request = _calendarService.Events.Insert(newEvent, "primary");
            var createdEvent = await request.ExecuteAsync();

            _logger.LogInformation("Calendar event created: {Title}", titulo);

            return JsonSerializer.Serialize(new
            {
                success = true,
                mensaje = "Evento creado exitosamente",
                evento = new
                {
                    id = createdEvent.Id,
                    titulo = createdEvent.Summary,
                    descripcion = createdEvent.Description,
                    fechaInicio = createdEvent.Start.DateTimeDateTimeOffset?.ToString("yyyy-MM-dd HH:mm"),
                    fechaFin = createdEvent.End.DateTimeDateTimeOffset?.ToString("yyyy-MM-dd HH:mm"),
                    ubicacion = createdEvent.Location,
                    enlace = createdEvent.HtmlLink
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create calendar event");
            return JsonSerializer.Serialize(new { success = false, error = $"Error al crear evento: {ex.Message}" });
        }
    }

    [Description("Obtiene los próximos eventos del calendario de Google Calendar")]
    public async Task<string> GetUpcomingEvents(
        [Description("Número máximo de eventos a devolver (por defecto 10)")] int maxResultados = 10)
    {
        if (_calendarService == null)
            return JsonSerializer.Serialize(new { success = false, error = "Calendar no está configurado" });

        maxResultados = Math.Clamp(maxResultados, 1, 50);

        try
        {
            var request = _calendarService.Events.List("primary");
            request.TimeMinDateTimeOffset = DateTime.Now.AddDays(-2);
            request.TimeMaxDateTimeOffset = DateTime.Now.AddMonths(12);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = maxResultados;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();

            if (events.Items == null || events.Items.Count == 0)
                return JsonSerializer.Serialize(new { success = true, mensaje = "No hay eventos próximos", eventos = Array.Empty<object>() });

            var eventList = events.Items.Select(e => new
            {
                id = e.Id,
                titulo = e.Summary,
                descripcion = e.Description,
                fechaInicio = e.Start.DateTimeDateTimeOffset?.ToString("yyyy-MM-dd HH:mm") ?? e.Start.Date,
                fechaFin = e.End.DateTimeDateTimeOffset?.ToString("yyyy-MM-dd HH:mm") ?? e.End.Date,
                ubicacion = e.Location,
                enlace = e.HtmlLink
            });

            return JsonSerializer.Serialize(new { success = true, total = eventList.Count(), eventos = eventList });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get upcoming events");
            return JsonSerializer.Serialize(new { success = false, error = $"Error al obtener eventos: {ex.Message}" });
        }
    }

    [Description("Obtiene todos los eventos de una fecha específica en formato yyyy-MM-dd. USA GetCurrentDateTime PRIMERO para saber la fecha actual.")]
    public async Task<string> GetEventsByDate(
        [Description("Fecha en formato yyyy-MM-dd (ejemplo: 2026-02-15)")] string fecha = "")
    {
        if (_calendarService == null)
            return JsonSerializer.Serialize(new { success = false, error = "Calendar no está configurado" });

        if (string.IsNullOrWhiteSpace(fecha))
            return JsonSerializer.Serialize(new { success = false, error = "La fecha es requerida en formato yyyy-MM-dd" });

        if (!DateTime.TryParse(fecha, out var targetDate))
            return JsonSerializer.Serialize(new { success = false, error = "Formato de fecha inválido. Use yyyy-MM-dd" });

        try
        {
            var request = _calendarService.Events.List("primary");
            request.TimeMinDateTimeOffset = targetDate.Date;
            request.TimeMaxDateTimeOffset = targetDate.Date.AddDays(1);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();

            if (events.Items == null || events.Items.Count == 0)
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    fecha,
                    mensaje = $"No hay eventos programados para {fecha}",
                    eventos = Array.Empty<object>()
                });
            }

            var eventList = events.Items.Select(e => new
            {
                id = e.Id,
                titulo = e.Summary,
                descripcion = e.Description,
                fechaInicio = e.Start.DateTimeDateTimeOffset?.ToString("yyyy-MM-dd HH:mm") ?? e.Start.Date,
                fechaFin = e.End.DateTimeDateTimeOffset?.ToString("yyyy-MM-dd HH:mm") ?? e.End.Date,
                ubicacion = e.Location,
                enlace = e.HtmlLink
            });

            return JsonSerializer.Serialize(new { success = true, fecha, total = eventList.Count(), eventos = eventList });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get events by date");
            return JsonSerializer.Serialize(new { success = false, error = $"Error al obtener eventos: {ex.Message}" });
        }
    }

    [Description("Elimina un evento del calendario usando su ID")]
    public async Task<string> DeleteEvent(
        [Description("ID del evento a eliminar")] string eventoId = "")
    {
        if (_calendarService == null)
            return JsonSerializer.Serialize(new { success = false, error = "Calendar no está configurado" });

        if (string.IsNullOrWhiteSpace(eventoId))
            return JsonSerializer.Serialize(new { success = false, error = "El ID del evento es requerido" });

        try
        {
            await _calendarService.Events.Delete("primary", eventoId).ExecuteAsync();

            _logger.LogInformation("Calendar event deleted: {EventId}", eventoId);

            return JsonSerializer.Serialize(new { success = true, mensaje = "Evento eliminado exitosamente", eventoId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete event");
            return JsonSerializer.Serialize(new { success = false, error = $"Error al eliminar evento: {ex.Message}" });
        }
    }

    public IEnumerable<AITool> AsAITools()
    {
        yield return AIFunctionFactory.Create(GetCurrentDateTime);

        if (_calendarService == null)
            yield break;

        yield return AIFunctionFactory.Create(CreateEvent);
        yield return AIFunctionFactory.Create(GetUpcomingEvents);
        yield return AIFunctionFactory.Create(GetEventsByDate);
        yield return AIFunctionFactory.Create(DeleteEvent);
    }
}
