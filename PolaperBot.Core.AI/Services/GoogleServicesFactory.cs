using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;
using PolaperBot.Core.AI.Configuration;

namespace PolaperBot.Core.AI.Services;

public interface IGoogleServicesFactory
{
    Task<GmailService?> CreateGmailServiceAsync();
    Task<CalendarService?> CreateCalendarServiceAsync();
}

public class GoogleServicesFactory : IGoogleServicesFactory
{
    private readonly GoogleOptions _options;
    private readonly ILogger<GoogleServicesFactory> _logger;
    private UserCredential? _credential;

    private static readonly string[] AllScopes =
    [
        GmailService.Scope.GmailSend,
        GmailService.Scope.GmailReadonly,
        CalendarService.Scope.Calendar,
        CalendarService.Scope.CalendarEvents
    ];

    public GoogleServicesFactory(GoogleOptions options, ILogger<GoogleServicesFactory> logger)
    {
        _options = options;
        _logger = logger;
    }

    private async Task<UserCredential?> GetCredentialAsync()
    {
        if (_credential != null)
            return _credential;

        if (!File.Exists(_options.CredentialsPath))
        {
            _logger.LogWarning("Google credentials not found at {Path}", _options.CredentialsPath);
            return null;
        }

        try
        {
            await using var stream = new FileStream(_options.CredentialsPath, FileMode.Open, FileAccess.Read);
            var secrets = (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets;

            Directory.CreateDirectory(_options.TokenFolder);
            var dataStore = new FileDataStore(_options.TokenFolder, true);
            var token = await dataStore.GetAsync<TokenResponse>("user");

            if (token != null)
            {
                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = secrets,
                    Scopes = AllScopes,
                    DataStore = dataStore
                });

                _credential = new UserCredential(flow, "user", token);

                if (token.IsStale)
                    await _credential.RefreshTokenAsync(CancellationToken.None);
            }
            else
            {
                var codeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = secrets,
                    Scopes = AllScopes,
                    DataStore = dataStore
                });

                var codeReceiver = new LocalServerCodeReceiver();
                var authCode = new AuthorizationCodeInstalledApp(codeFlow, codeReceiver);
                _credential = await authCode.AuthorizeAsync("user", CancellationToken.None);

                _logger.LogInformation("Google authorization completed successfully");
            }

            return _credential;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Google credentials");
            return null;
        }
    }

    public async Task<GmailService?> CreateGmailServiceAsync()
    {
        if (!_options.EnableGmail)
            return null;

        var credential = await GetCredentialAsync();
        if (credential == null)
            return null;

        return new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "PolaperBot"
        });
    }

    public async Task<CalendarService?> CreateCalendarServiceAsync()
    {
        if (!_options.EnableCalendar)
            return null;

        var credential = await GetCredentialAsync();
        if (credential == null)
            return null;

        return new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "PolaperBot"
        });
    }
}
