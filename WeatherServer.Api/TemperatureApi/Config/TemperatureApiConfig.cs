namespace WeatherServer.Api.TemperatureApi.Config;

public sealed class TemperatureApiConfig
{
    public TemperatureApiInputs Inputs { get; set; } = new();
    public TemperatureApiOutputs Outputs { get; set; } = new();
}
