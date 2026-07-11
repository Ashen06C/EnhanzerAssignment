using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Enhanzer.Assignment.Application.Auth;
using Enhanzer.Assignment.Application.Common;
using Enhanzer.Assignment.Application.Locations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enhanzer.Assignment.Infrastructure.ExternalAuth;

public sealed class ExternalAuthClient(
    HttpClient httpClient,
    IOptions<ExternalApiOptions> options,
    ILogger<ExternalAuthClient> logger) : IExternalAuthClient
{
    private readonly ExternalApiOptions options = options.Value;

    public async Task<ExternalLoginResult> LoginAsync(
        ExternalLoginRequest request,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));

        var payload = new
        {
            API_Action = "GetLoginData",
            Device_Id = options.DeviceId,
            Sync_Time = string.Empty,
            Company_Code = request.Email,
            API_Body = new
            {
                Username = request.Email,
                Pw = request.Password
            }
        };

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(options.BaseUrl, payload, timeout.Token);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExternalServiceException("The external authentication service timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException("The external authentication service is unavailable.", ex);
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new AuthenticationFailedException();
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException("The external authentication service returned an unsuccessful status.");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            using var document = JsonDocument.Parse(json);
            if (TryFindUserLocations(document.RootElement, out var locations))
            {
                return new ExternalLoginResult(request.Email, locations);
            }

            if (LooksLikeAuthenticationFailure(document.RootElement))
            {
                throw new AuthenticationFailedException();
            }

            logger.LogWarning("External login response did not contain User_Locations.");
            throw new ExternalServiceException("The external authentication response was not usable.");
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException("The external authentication response was not valid JSON.", ex);
        }
    }

    private static bool TryFindUserLocations(JsonElement element, out IReadOnlyCollection<LocationDto> locations)
    {
        locations = Array.Empty<LocationDto>();

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, "User_Locations", StringComparison.OrdinalIgnoreCase) &&
                    property.Value.ValueKind == JsonValueKind.Array)
                {
                    locations = property.Value
                        .EnumerateArray()
                        .Select(ReadLocation)
                        .Where(location => location is not null)
                        .Select(location => location!)
                        .ToList();
                    return true;
                }

                if (TryFindUserLocations(property.Value, out locations))
                {
                    return true;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (TryFindUserLocations(item, out locations))
                {
                    return true;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var value = element.GetString();
            if (!string.IsNullOrWhiteSpace(value) && value.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                using var nested = JsonDocument.Parse(value);
                return TryFindUserLocations(nested.RootElement, out locations);
            }
        }

        return false;
    }

    private static LocationDto? ReadLocation(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var code = TryReadString(element, "Location_Code");
        var name = TryReadString(element, "Location_Name");

        return string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)
            ? null
            : new LocationDto(code.Trim(), name.Trim());
    }

    private static string? TryReadString(JsonElement element, string propertyName)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value.GetString();
            }
        }

        return null;
    }

    private static bool LooksLikeAuthenticationFailure(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, "Status_Code", StringComparison.OrdinalIgnoreCase) &&
                    property.Value.TryGetInt32(out var statusCode) &&
                    statusCode == 401)
                {
                    return true;
                }
            }
        }

        var text = element.GetRawText();
        return text.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("incorrect", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
               text.Contains("un-authorize", StringComparison.OrdinalIgnoreCase);
    }
}
