namespace WeatherServer.Api.TemperatureApi.Config;

public sealed class TemperatureApiInputs
{
    public ICollection<string> Cities { get; set; } = new List<string>();
}
