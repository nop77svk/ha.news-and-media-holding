namespace WeatherServer.Api.TemperatureApi.Services.ExternalWeatherApi;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Path;
using Microsoft.Extensions.Options;
using WeatherServer.Api.Infrastructure;
using WeatherServer.Api.TemperatureApi.Services.ExternalWeatherApi.ResponseParser;

public class GenericConfigurableWeatherApiClient
    : IExternalWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<Config.WeatherService.TemperatureApiWeatherServiceConfig> _weatherServiceConfig;

    public GenericConfigurableWeatherApiClient(HttpClient httpClient, IOptions<Config.WeatherService.TemperatureApiWeatherServiceConfig> weatherServiceConfig)
    {
        _httpClient = httpClient;
        _weatherServiceConfig = weatherServiceConfig;
    }

    public async Task<ExternalWeatherApiData> GetCurrentWeatherDataAsync(string cityName, CancellationToken cancellationToken)
    {
        Uri uri = GetExternalWeatherServiceUri(cityName);
        Stream extApiResponse = await _httpClient.GetStreamAsync(uri, cancellationToken);

        IExternalWeatherApiResponseParser responseParser = new GenericJsonResponseParser(extApiResponse, _weatherServiceConfig);
        decimal temperature = await responseParser.GetTemperatureAsync(cancellationToken);
        DateTime measuredTimeStamp = await responseParser.GetMeasuredTimeStampAsync(cancellationToken);

        return new ExternalWeatherApiData(temperature, measuredTimeStamp);
    }

    private Uri GetExternalWeatherServiceUri(string cityName)
    {
        if (!_weatherServiceConfig.Value.InputToApiInput.TryGetValue(cityName, out var cityLocation))
        {
            throw new ConfigurationException($"Unable to map input '{cityName}' to external API request input.");
        }

        string? extApiApiKey = Environment.GetEnvironmentVariable(_weatherServiceConfig.Value.ApiKeyEnvVar);
        if (string.IsNullOrWhiteSpace(extApiApiKey))
        {
            throw new ConfigurationException($"API key environment variable '{_weatherServiceConfig.Value.ApiKeyEnvVar}' is not set.");
        }

        string extApiUriStringWoSecrects = _weatherServiceConfig.Value.RequestUriTemplate
            .Replace(@"{lat}", cityLocation.Lat.ToString(CultureInfo.InvariantCulture))
            .Replace(@"{lon}", cityLocation.Lon.ToString(CultureInfo.InvariantCulture));

        string extApiUriString = extApiUriStringWoSecrects.Replace(@"{apiKey}", extApiApiKey);
        string extApiUriStringObfuscated = extApiUriStringWoSecrects.Replace(@"{apiKey}", "***REDACTED***");
        if (!Uri.TryCreate(extApiUriString, UriKind.Absolute, out Uri? result) || result is null)
        {
            throw new ConfigurationException($"The constructed external API URI '{extApiUriStringObfuscated}' is not a valid absolute URI.");
        }

        return result;
    }
}
