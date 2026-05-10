namespace WeatherServer.Api.TemperatureApi.Config.Inputs;

public sealed class TemperatureApiInputs
{
    public HashSet<string> Cities { get; set; } = new HashSet<string>();
}
