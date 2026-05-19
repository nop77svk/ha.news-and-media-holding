namespace WeatherServer.Api.TemperatureApi.Services.ExternalWeatherApi;

public interface IExternalWeatherApiClient
{
    Task<ExternalWeatherApiData> GetCurrentWeatherDataAsync(string cityName, CancellationToken cancellationToken);
}
