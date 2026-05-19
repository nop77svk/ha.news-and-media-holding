namespace WeatherServer.Api.TemperatureApi.Services.ResponseParser;

public interface IExternalWeatherApiResponseParser
{
    Task<decimal> GetTemperatureAsync(CancellationToken cancellationToken);
    Task<DateTime> GetMeasuredTimeStampAsync(CancellationToken cancellationToken);
}
