using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly JsonSerializerOptions ExternalRequestJsonOptions = new()
    {
        PropertyNamingPolicy = null
    };

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

        var jsonPayload = JsonSerializer.Serialize(payload, ExternalRequestJsonOptions);
        using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync(options.BaseUrl, content, timeout.Token);
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
            LogExternalFailureIfPresent(json);

            var typedResult = TryReadTypedLoginResult(json, request.Email);
            if (typedResult is not null)
            {
                return typedResult;
            }

            using var document = JsonDocument.Parse(json);
            if (LooksLikeAuthenticationFailure(document.RootElement))
            {
                throw new AuthenticationFailedException();
            }

            if (TryFindUserLocations(document.RootElement, out var locations) && locations.Count > 0)
            {
                return new ExternalLoginResult(request.Email, locations);
            }

            if (TryFindUserLocations(document.RootElement, out _))
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

    private void LogExternalFailureIfPresent(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!TryReadInt32(document.RootElement, "Status_Code", out var statusCode) || statusCode == 200)
            {
                return;
            }

            var message = TryReadString(document.RootElement, "Message");
            logger.LogWarning(
                "External login returned Status_Code {StatusCode}. Message: {Message}",
                statusCode,
                message ?? "No message returned.");
        }
        catch (JsonException)
        {
            logger.LogWarning("External login returned non-JSON response.");
        }
    }

    private static ExternalLoginResult? TryReadTypedLoginResult(string json, string fallbackEmail)
    {
        ExternalLoginResponse? response;
        try
        {
            response = JsonSerializer.Deserialize<ExternalLoginResponse>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (response is null)
        {
            return null;
        }

        if (response.StatusCode != 0 && response.StatusCode != 200)
        {
            throw new AuthenticationFailedException();
        }

        if (response.ResponseBody is null || response.ResponseBody.Count == 0)
        {
            return null;
        }

        var user = response.ResponseBody[0];
        if (user.UserLocations is null)
        {
            return null;
        }

        var locations = user.UserLocations
            .Where(location => !string.IsNullOrWhiteSpace(location.LocationCode) &&
                               !string.IsNullOrWhiteSpace(location.LocationName))
            .Select(location => new LocationDto(location.LocationCode.Trim(), location.LocationName.Trim()))
            .ToList();

        if (locations.Count == 0)
        {
            throw new AuthenticationFailedException();
        }

        var email = string.IsNullOrWhiteSpace(user.Email) ? fallbackEmail : user.Email.Trim();
        return new ExternalLoginResult(email, locations);
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
            if (!string.IsNullOrWhiteSpace(value))
            {
                var trimmed = value.TrimStart();
                if (trimmed.StartsWith("{", StringComparison.Ordinal) ||
                    trimmed.StartsWith("[", StringComparison.Ordinal))
                {
                    using var nested = JsonDocument.Parse(value);
                    return TryFindUserLocations(nested.RootElement, out locations);
                }
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

    private static bool TryReadInt32(JsonElement element, string propertyName, out int value)
    {
        value = default;

        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value.TryGetInt32(out value);
            }
        }

        return false;
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

    private sealed class ExternalLoginResponse
    {
        [JsonPropertyName("Status_Code")]
        public int StatusCode { get; set; }

        [JsonPropertyName("Response_Body")]
        public List<ExternalLoginResponseBody>? ResponseBody { get; set; }
    }

    private sealed class ExternalLoginResponseBody
    {
        [JsonPropertyName("Email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("User_Locations")]
        public List<ExternalLocation>? UserLocations { get; set; }
    }

    private sealed class ExternalLocation
    {
        [JsonPropertyName("Location_Code")]
        public string LocationCode { get; set; } = string.Empty;

        [JsonPropertyName("Location_Name")]
        public string LocationName { get; set; } = string.Empty;
    }
}
