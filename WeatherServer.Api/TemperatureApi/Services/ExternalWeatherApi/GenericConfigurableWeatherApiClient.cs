namespace WeatherServer.Api.TemperatureApi.Services.ExternalWeatherApi;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Path;
using Microsoft.Extensions.Options;
using WeatherServer.Api.Infrastructure;
using WeatherServer.Api.TemperatureApi.Config.WeatherService;

public class GenericConfigurableWeatherApiClient
    : IExternalWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<Config.WeatherService.TemperatureApiWeatherServiceConfig> _weatherServiceConfig;

    private readonly JsonPath _temperatureJsonPath;
    private readonly JsonPath? _measuredTimeStampJsonPath;

    public GenericConfigurableWeatherApiClient(HttpClient httpClient, IOptions<Config.WeatherService.TemperatureApiWeatherServiceConfig> weatherServiceConfig)
    {
        _httpClient = httpClient;
        _weatherServiceConfig = weatherServiceConfig;

        _temperatureJsonPath = JsonPath.TryParse(_weatherServiceConfig.Value.ApiResponseToResponse.Temperature, out var parsedTemperatureJsonPath) && parsedTemperatureJsonPath is not null
            ? parsedTemperatureJsonPath
            : throw new ConfigurationException($"The configured JSON path for extracting the temperature from the external API response ('{_weatherServiceConfig.Value.ApiResponseToResponse.Temperature}') is not valid.");

        _measuredTimeStampJsonPath = JsonPath.TryParse(_weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStamp, out var parsedMeasuredTimeStampJsonPath)
            ? parsedMeasuredTimeStampJsonPath
            : throw new ConfigurationException($"The configured JSON path for extracting the temperature measurement timestamp from the external API response ('{_weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStamp}') is not valid.");
    }

    private static readonly Regex _rxStripSpecialCharacters = new(@"[^a-zA-Z0-9]", RegexOptions.Compiled);

    public async Task<ExternalWeatherApiData> GetCurrentWeatherDataAsync(string cityName, CancellationToken cancellationToken)
    {
        Uri extApiUri = GetExternalWeatherServiceUri(cityName);
        Stream extApiResponse = await _httpClient.GetStreamAsync(extApiUri, cancellationToken);

        JsonNode extApiResponseJson = await JsonNode.ParseAsync(extApiResponse, cancellationToken: cancellationToken)
            ?? throw new ExternalWeatherApiCallException("External Weather API response could not be parsed as JSON.");

        decimal temperature = GetTemperatureFromJsonResponse(extApiResponseJson);
        DateTime measuredTimeStamp = GetMeasuredTimeStampFromJsonResponse(extApiResponseJson);
        return new ExternalWeatherApiData(temperature, measuredTimeStamp);
    }

    private DateTime GetMeasuredTimeStampFromJsonResponse(JsonNode extApiResponseJson)
    {
        DateTime result;

        bool isMeasureTimeStampConfigValueUnixEpochSeconds = _rxStripSpecialCharacters.Replace(_weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStampFormat, string.Empty)
            .Equals(TemperatureApiWeatherServiceResponseMapConfig.MeasureTimeStampFormatUnixEpoch, StringComparison.OrdinalIgnoreCase);

        var jsonNodeValue = _measuredTimeStampJsonPath?.Evaluate(extApiResponseJson)
            ?.Matches
            ?.FirstOrDefault()
            ?.Value
            ?? null;

        if (jsonNodeValue is null)
        {
            result = DateTime.Now;
        }
        else if (jsonNodeValue.GetValueKind() == JsonValueKind.Number)
        {
            long measuredTimeStampAsUnixEpoch = jsonNodeValue.TryGetValue<long>(out var parsedLongNodeValue)
                ? parsedLongNodeValue
                : throw new ExternalWeatherApiCallException($"The extracted measurement timestamp value '{jsonNodeValue}' is not a valid integer number representing seconds since the Unix epoch.");

            if (!isMeasureTimeStampConfigValueUnixEpochSeconds)
            {
                throw new ConfigurationException($"The extracted measurement timestamp value from the external API response is a number representing seconds since the Unix epoch, but the configured measurement timestamp format '{_weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStampFormat}' does not indicate that the value is in Unix epoch seconds. Please check the configuration of the measurement timestamp format.");
            }

            result = DateTime.UnixEpoch.AddSeconds(measuredTimeStampAsUnixEpoch);
        }
        else if (jsonNodeValue.GetValueKind() == JsonValueKind.String)
        {
            var measuredTimeStampStr = jsonNodeValue.GetValue<string>();

            if (isMeasureTimeStampConfigValueUnixEpochSeconds)
            {
                long measuredTimeStampAsUnixEpoch = long.TryParse(measuredTimeStampStr, NumberStyles.Any, CultureInfo.InvariantCulture, out long parsedMeasuredTimeStamp)
                    ? parsedMeasuredTimeStamp
                    : throw new ExternalWeatherApiCallException($"The extracted measurement timestamp value '{measuredTimeStampStr}' is not a valid decimal number representing seconds since the Unix epoch, even though the configured measurement timestamp format '{_weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStampFormat}' indicates that the value should be in Unix epoch seconds.");

                result = DateTime.UnixEpoch.AddSeconds(measuredTimeStampAsUnixEpoch);
            }
            else
            {
                result = DateTime.TryParseExact(measuredTimeStampStr, _weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedMeasuredTimeStamp)
                    ? parsedMeasuredTimeStamp
                    : throw new ExternalWeatherApiCallException($"The extracted measurement timestamp value '{measuredTimeStampStr}' does not match the expected format '{_weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStampFormat}'.");
            }
        }
        else
        {
            throw new ExternalWeatherApiCallException($"The extracted measurement timestamp value has an unsupported JSON value kind '{jsonNodeValue.GetValueKind()}'. Expected a number or a string.");
        }

        return result;
    }

    private decimal GetTemperatureFromJsonResponse(JsonNode extApiResponseJson) =>
            _temperatureJsonPath.Evaluate(extApiResponseJson).Matches.FirstOrDefault()?.Value?.GetValue<decimal>()
                ?? throw new ExternalWeatherApiCallException($"Could not extract temperature from external API response using JSON path '{_weatherServiceConfig.Value.ApiResponseToResponse.Temperature}'.");

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
