namespace WeatherServer.Api.TemperatureApi.Services.ResponseParser;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.Path;
using Microsoft.Extensions.Options;
using WeatherServer.Api.Infrastructure;
using WeatherServer.Api.TemperatureApi.Config.WeatherService;
using WeatherServer.Api.TemperatureApi.Services.ExternalWeatherApi;

public class GenericJsonResponseParser : IExternalWeatherApiResponseParser
{
    private static readonly Regex _rxStripSpecialCharacters = new(@"[^a-zA-Z0-9]", RegexOptions.Compiled);

    private readonly IOptions<TemperatureApiWeatherServiceConfig> _weatherServiceConfig;

    private readonly JsonPath _temperatureJsonPath;
    private readonly JsonPath? _measuredTimeStampJsonPath;
    private bool _isMeasureTimeStampConfigValueUnixEpochSeconds;

    private readonly Stream _responseStream;
    private JsonNode? _parsedResponse = null;

    public GenericJsonResponseParser(Stream responseStream, IOptions<Config.WeatherService.TemperatureApiWeatherServiceConfig> weatherServiceConfig)
    {
        _responseStream = responseStream;
        _weatherServiceConfig = weatherServiceConfig;

        _temperatureJsonPath = JsonPath.TryParse(_weatherServiceConfig.Value.ApiResponseToResponse.Temperature, out var parsedTemperatureJsonPath) && parsedTemperatureJsonPath is not null
            ? parsedTemperatureJsonPath
            : throw new ConfigurationException($"The configured JSON path for extracting the temperature from the external API response ('{_weatherServiceConfig.Value.ApiResponseToResponse.Temperature}') is not valid.");

        _measuredTimeStampJsonPath = JsonPath.TryParse(_weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStamp, out var parsedMeasuredTimeStampJsonPath)
            ? parsedMeasuredTimeStampJsonPath
            : throw new ConfigurationException($"The configured JSON path for extracting the temperature measurement timestamp from the external API response ('{_weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStamp}') is not valid.");

        _isMeasureTimeStampConfigValueUnixEpochSeconds = _rxStripSpecialCharacters.Replace(_weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStampFormat, string.Empty)
            .Equals(TemperatureApiWeatherServiceResponseMapConfig.MeasureTimeStampFormatUnixEpoch, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<DateTime> GetMeasuredTimeStampAsync(CancellationToken cancellationToken)
    {
        DateTime result;
        JsonNode responseJson = await GetParsedResponse(cancellationToken);

        var jsonNodeValue = _measuredTimeStampJsonPath?.Evaluate(responseJson)
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

            if (!_isMeasureTimeStampConfigValueUnixEpochSeconds)
            {
                throw new ConfigurationException($"The extracted measurement timestamp value from the external API response is a number representing seconds since the Unix epoch, but the configured measurement timestamp format '{_weatherServiceConfig.Value.ApiResponseToResponse.MeasureTimeStampFormat}' does not indicate that the value is in Unix epoch seconds. Please check the configuration of the measurement timestamp format.");
            }

            result = DateTime.UnixEpoch.AddSeconds(measuredTimeStampAsUnixEpoch);
        }
        else if (jsonNodeValue.GetValueKind() == JsonValueKind.String)
        {
            var measuredTimeStampStr = jsonNodeValue.GetValue<string>();

            if (_isMeasureTimeStampConfigValueUnixEpochSeconds)
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

    public async Task<decimal> GetTemperatureAsync(CancellationToken cancellationToken)
    {
        decimal result;
        JsonNode responseJson = await GetParsedResponse(cancellationToken);

        var jsonNodeValue = _temperatureJsonPath?.Evaluate(responseJson)
            ?.Matches
            ?.FirstOrDefault()
            ?.Value
            ?? null;

        if (jsonNodeValue is null)
        {
            throw new ExternalWeatherApiCallException($"Could not extract a temperature value from the external API response using the configured JSON path '{_weatherServiceConfig.Value.ApiResponseToResponse.Temperature}'.");
        }
        else if (jsonNodeValue?.GetValueKind() == JsonValueKind.Number)
        {
            result = jsonNodeValue.GetValue<decimal>();
        }
        else
        {
            throw new ExternalWeatherApiCallException($"The extracted temperature value has an unsupported JSON value kind '{jsonNodeValue.GetValueKind()}'. Expected a number.");
        }

        return result;
    }

    private async Task<JsonNode> GetParsedResponse(CancellationToken cancellationToken)
    {
        if (_parsedResponse is null)
        {
            _parsedResponse = await JsonNode.ParseAsync(_responseStream, cancellationToken: cancellationToken);
        }

        return _parsedResponse ?? throw new ExternalWeatherApiCallException("External Weather API response could not be parsed as JSON.");
    }
}
