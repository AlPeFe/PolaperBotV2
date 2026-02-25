using PolaperBot.Core.AI.Services;

namespace PolaperBot.Api.Endpoints;

public static class ChatEndpoints
{
    private const long TestSessionId = 999999;

    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat");

        group.MapPost("/", async (
            ChatRequest request,
            IAgentService agentService,
            CancellationToken cancellationToken) =>
        {
            var response = await agentService.SendMessageAsync(request.UserId, request.Message, cancellationToken);
            return Results.Ok(new ChatResponse(response));
        })
        .WithName("SendMessage");

        group.MapPost("/human", async (
            HumanChatRequest request,
            IAgentService agentService,
            CancellationToken cancellationToken) =>
        {
            var response = await agentService.SendMessageAsync(TestSessionId, request.Message, cancellationToken);
            return Results.Ok(new ChatResponse(response));
        })
        .WithName("HumanChat");

        group.MapGet("/health", () => Results.Ok("PolaperBot API is running"))
            .WithName("HealthCheck");
    }
}

public record ChatRequest(long UserId, string Message);

public record HumanChatRequest(string Message);

public record ChatResponse(string Response);
