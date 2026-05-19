namespace WeatherServer.Api.TemperatureApi.Config.WeatherService;

public sealed class TemperatureApiWeatherServiceResponseMapConfig
{
    public const string MeasureTimeStampFormatUnixEpoch = @"UnixEpoch";

    public string Temperature { get; set; } = string.Empty;
    public string MeasureTimeStamp { get; set; } = string.Empty;
    public string MeasureTimeStampFormat { get; set; } = string.Empty;
}
