using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PolaperBot.Core.AI.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PolaperBot.Api.Endpoints;

public static class TelegramEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static void MapTelegramEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/telegramwebhook", async (
            HttpRequest request,
            [FromServices] ITelegramBotClient telegramBotClient,
            [FromServices] IAgentService agentService,
            CancellationToken cancellationToken) =>
        {
            var update = await JsonSerializer.DeserializeAsync<Update>(request.Body, JsonOptions, cancellationToken);

            if (update?.Message is null)
                return Results.BadRequest("No message in update");

            var response = await agentService.SendMessageAsync(
                update.Message.Chat.Id,
                update.Message.Text ?? string.Empty,
                cancellationToken);

            await telegramBotClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: response,
                cancellationToken: cancellationToken);

            return Results.Ok();
        })
        .WithName("TelegramWebhook");
    }
}
