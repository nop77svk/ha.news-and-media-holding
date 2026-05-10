namespace WeatherServer.Api.TemperatureApi.Config;

public sealed class TemperatureApiInputs
{
    public HashSet<string> Cities { get; set; } = new HashSet<string>();
}
