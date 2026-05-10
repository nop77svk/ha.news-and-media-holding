namespace WeatherServer.Api.TemperatureApi.Config;

public sealed class TemperatureApiWeatherServiceResponseMapConfig
{
    public string Temperature { get; set; } = string.Empty;
    public string MeasureTimeStamp { get; set; } = string.Empty;
    public string MeasureTimeStampFormat { get; set; } = string.Empty;
}
