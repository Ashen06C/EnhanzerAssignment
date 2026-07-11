namespace Enhanzer.Assignment.Infrastructure.ExternalAuth;

public sealed class ExternalApiOptions
{
    public const string SectionName = "ExternalApi";

    public string BaseUrl { get; set; } = "https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke";
    public string DeviceId { get; set; } = "D001";
    public int TimeoutSeconds { get; set; } = 20;
}
