namespace WeatherServer.Api.TemperatureApi;

using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

using WeatherServer.Api.Infrastructure;
using WeatherServer.Api.TemperatureApi.Services.ExternalWeatherApi;

[ApiController]
[Route(ApiConstants.TemperatureApiRoot)]
public class TemperatureController : ControllerBase
{
    private readonly IExternalWeatherApiClient _weatherApiClient;
    private readonly IOptions<Config.Outputs.TemperatureApiOutputs> _outputsConfig;
    private readonly IOptions<Config.Inputs.TemperatureApiInputs> _inputsConfig;

    private string ApiGetResultOutputFormat => $"{{0:F{_outputsConfig.Value.Temperature.Decimals}}}";

    public TemperatureController(
        IExternalWeatherApiClient weatherApiClient,
        IOptions<Config.Outputs.TemperatureApiOutputs> outputsConfig,
        IOptions<Config.Inputs.TemperatureApiInputs> inputsConfig)
    {
        _outputsConfig = outputsConfig;
        _inputsConfig = inputsConfig;
        _weatherApiClient = weatherApiClient;
    }

    [HttpGet("{cityName}")]
    public async Task<ActionResult<string>> GetTemperature([FromRoute] string cityName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_inputsConfig.Value.Cities.Contains(cityName, StringComparer.InvariantCultureIgnoreCase))
        {
            return NotFound($"City `{cityName}` not recognised");
        }

        var weatherApiClientResponse = await _weatherApiClient.GetCurrentWeatherDataAsync(cityName, cancellationToken);

        string resultFormatted = string.Format(CultureInfo.InvariantCulture, ApiGetResultOutputFormat, weatherApiClientResponse.TemperatureCelsius);
        return Ok(resultFormatted);
    }
}
