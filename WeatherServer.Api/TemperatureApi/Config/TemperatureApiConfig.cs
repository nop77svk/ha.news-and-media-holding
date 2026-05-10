namespace WeatherServer.Api.TemperatureApi.Config;

public sealed class TemperatureApiConfig
{
    public Inputs.TemperatureApiInputs Inputs { get; set; } = new();
    public WeatherService.TemperatureApiWeatherServiceConfig WeatherService { get; set; } = new();
    public Outputs.TemperatureApiOutputs Outputs { get; set; } = new();
}
