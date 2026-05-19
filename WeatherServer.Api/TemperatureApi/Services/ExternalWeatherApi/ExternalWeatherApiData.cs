#pragma warning disable SA1313
namespace WeatherServer.Api.TemperatureApi.Services.ExternalWeatherApi;

public record ExternalWeatherApiData(decimal TemperatureCelsius, DateTime MeasuredAt)
{
}
