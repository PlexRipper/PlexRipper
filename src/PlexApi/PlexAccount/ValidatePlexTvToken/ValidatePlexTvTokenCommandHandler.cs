using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using FastEndpoints;
using FluentValidation;
using Reaparr.Domain;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi;

public class ValidatePlexTokenCommandValidator : Validator<ValidatePlexTokenCommand>
{
    public ValidatePlexTokenCommandValidator()
    {
        RuleFor(x => x.AuthenticationToken).MinimumLength(5);
    }
}

public class ValidatePlexTvTokenCommandHandler
    : ICommandHandler<ValidatePlexTokenCommand, Result<ValidatePlexTokenCommandResult>>
{
    private readonly IPlexApiClientFactory _plexApiClientFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _log;

    public ValidatePlexTvTokenCommandHandler(
        ILogger log,
        IPlexApiClientFactory plexApiClientFactory,
        IHttpClientFactory httpClientFactory
    )
    {
        _log = log.ForContext<ValidatePlexTvTokenCommandHandler>();

        _plexApiClientFactory = plexApiClientFactory;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<ValidatePlexTokenCommandResult>> ExecuteAsync(
        ValidatePlexTokenCommand command,
        CancellationToken ct
    )
    {
        var clientId = Guid.NewGuid().ToString();

        var httpClient = _httpClientFactory.CreateClient();

        // TODO This is a temporary fix until the C# SDK is updated to support the new Plex API endpoints.
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://plex.tv/api/v2/user?X-Plex-Token={command.AuthenticationToken}"
        );
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (TaskCanceledException)
        {
            return Result.Fail("Request timed out").Add408RequestTimeoutError();
        }
        catch (HttpRequestException e)
        {
            return Result.Fail(new ExceptionalError(e)).Add502BadGatewayError("HTTP request failed");
        }

        var statusCode = (int)httpResponse.StatusCode;

        if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return Result.Fail("Unauthorized").AddPlex401UnauthorizedError();
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            var errorBody = await httpResponse.Content.ReadAsStringAsync(ct);
            return Result
                .Fail(string.IsNullOrWhiteSpace(errorBody) ? "Request failed" : errorBody)
                .AddStatusCode(statusCode);
        }

        var json = await httpResponse.Content.ReadAsStringAsync(ct);

        PlexUserAccount? account;
        try
        {
            account = JsonSerializer.Deserialize<PlexUserAccount>(json, DefaultJsonSerializerOptions.ConfigStandard);
        }
        catch (JsonException e)
        {
            return Result
                .Fail(new ExceptionalError(e))
                .AddStatusCode(statusCode, "Failed to parse Plex user JSON response");
        }

        if (account is null)
        {
            return Result.Fail("Empty Plex user response").AddStatusCode(statusCode);
        }

        var isValidated = httpResponse.IsSuccessStatusCode;

        var mapped = new ValidatePlexTokenCommandResult
        {
            ClientId = clientId,
            Username = account.Username ?? string.Empty,
            PlexId = account.Id,
            Uuid = account.Uuid ?? string.Empty,
            IsValidated = isValidated,
            ValidatedAt = isValidated ? DateTime.UtcNow : null,
            Title = account.Title ?? string.Empty,
            Email = account.Email ?? string.Empty,
            AuthenticationToken = account.AuthToken ?? command.AuthenticationToken,
            Is2Fa = account.TwoFactorEnabled.GetValueOrDefault(),
        };

        _log.Here()
            .Information(
                "Successfully retrieved the PlexAccount data for user {UserName} from the PlexApi",
                mapped.Username
            );

        return Result.Ok(mapped).AddStatusCode(statusCode);
    }
}

file sealed class PlexUserAccount
{
    public string? Username { get; set; }
    public long Id { get; set; }
    public string? Uuid { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? AuthToken { get; set; }
    public bool? TwoFactorEnabled { get; set; }
}
