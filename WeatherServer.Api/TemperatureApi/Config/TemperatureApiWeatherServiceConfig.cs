namespace WeatherServer.Api.TemperatureApi.Config;

public sealed class TemperatureApiWeatherServiceConfig
{
    public string URI { get; set; } = string.Empty;
    public string ApiKeyEnvVar { get; set; } = string.Empty;
    public Dictionary<string, TemperatureApiWeatherServiceInputMapConfig> InputToApiInput { get; set; } = new();
    public string ResponseFormat { get; set; } = "json";
    public TemperatureApiWeatherServiceResponseMapConfig ApiResponseToResponse { get; set; } = new();
}
