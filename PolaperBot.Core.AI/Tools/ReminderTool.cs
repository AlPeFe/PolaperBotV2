using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.AI;
using PolaperBot.Core.AI.Reminders;

namespace PolaperBot.Core.AI.Tools;

public class ReminderTool
{
    private readonly IReminderRepository _repository;

    public ReminderTool(IReminderRepository repository)
    {
        _repository = repository;
    }

    [Description("Crea un recordatorio. El agente te avisará por Telegram cuando llegue el momento. " +
                 "El scheduledFor debe ser una fecha/hora ISO 8601 (ej: '2024-12-25T10:00:00'). " +
                 "Primero usa GetCurrentDateTime para obtener la fecha/hora actual y calcula la fecha objetivo.")]
    public async Task<string> CreateReminder(
        [Description("Descripción del recordatorio (qué debe recordar el agente)")] string description,
        [Description("Fecha y hora ISO 8601 cuando debe avisar (ej: '2024-12-25T10:00:00')")] string scheduledFor,
        [Description("ID del chat de Telegram donde enviar el aviso")] long telegramChatId)
    {
        try
        {
            if (!DateTime.TryParse(scheduledFor, out var scheduledDate))
            {
                return JsonSerializer.Serialize(new { success = false, error = "Formato de fecha inválido. Usa ISO 8601 (ej: '2024-12-25T10:00:00')" });
            }

            if (scheduledDate <= DateTime.UtcNow)
            {
                return JsonSerializer.Serialize(new { success = false, error = "La fecha debe ser en el futuro" });
            }

            var reminder = new Reminder
            {
                Description = description,
                ScheduledFor = scheduledDate,
                CreatedAt = DateTime.UtcNow,
                TelegramChatId = telegramChatId,
                IsNotified = false
            };

            var id = await _repository.CreateAsync(reminder);

            return JsonSerializer.Serialize(new
            {
                success = true,
                reminderId = id,
                description = description,
                scheduledFor = scheduledDate.ToString("yyyy-MM-dd HH:mm:ss"),
                localTime = scheduledDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { success = false, error = ex.Message });
        }
    }

    [Description("Obtiene la fecha y hora actual en UTC para poder calcular fechas futuras para los recordatorios")]
    public string GetCurrentDateTime()
    {
        var now = DateTime.UtcNow;
        return JsonSerializer.Serialize(new
        {
            utcNow = now.ToString("O"),
            utcFormatted = now.ToString("yyyy-MM-dd HH:mm:ss"),
            localNow = now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
            localFormatted = now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
            dayOfWeek = now.DayOfWeek.ToString(),
            dayName = now.ToString("dddd"),
            date = now.ToString("yyyy-MM-dd"),
            time = now.ToString("HH:mm:ss")
        });
    }

    [Description("Lista todos los recordatorios pendientes")]
    public async Task<string> ListPendingReminders()
    {
        var reminders = await _repository.GetPendingAsync();
        
        return JsonSerializer.Serialize(new
        {
            count = reminders.Count,
            reminders = reminders.Select(r => new
            {
                id = r.Id,
                description = r.Description,
                scheduledFor = r.ScheduledFor.ToString("yyyy-MM-dd HH:mm:ss"),
                localTime = r.ScheduledFor.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                chatId = r.TelegramChatId
            })
        });
    }

    [Description("Elimina un recordatorio por su ID")]
    public async Task<string> DeleteReminder(
        [Description("ID del recordatorio a eliminar")] int reminderId)
    {
        var reminder = await _repository.GetByIdAsync(reminderId);
        if (reminder == null)
        {
            return JsonSerializer.Serialize(new { success = false, error = "Recordatorio no encontrado" });
        }

        await _repository.DeleteAsync(reminderId);
        return JsonSerializer.Serialize(new { success = true, message = $"Recordatorio '{reminder.Description}' eliminado" });
    }

    public IEnumerable<AITool> AsAITools()
    {
        yield return AIFunctionFactory.Create(CreateReminder);
        yield return AIFunctionFactory.Create(GetCurrentDateTime);
        yield return AIFunctionFactory.Create(ListPendingReminders);
        yield return AIFunctionFactory.Create(DeleteReminder);
    }
}
