namespace WeatherServer.Api.TemperatureApi.Services.ExternalWeatherApi.ResponseParser;

public interface IExternalWeatherApiResponseParser
{
    Task<decimal> GetTemperatureAsync(CancellationToken cancellationToken);
    Task<DateTime> GetMeasuredTimeStampAsync(CancellationToken cancellationToken);
}
