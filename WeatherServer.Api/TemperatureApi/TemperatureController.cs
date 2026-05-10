namespace WeatherServer.Api.TemperatureApi;

using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using WeatherServer.Api.Infrastructure;
using WeatherServer.Api.TemperatureApi.Config;

[ApiController]
[Route(ApiConstants.TemperatureApiRoot)]
public class TemperatureController : ControllerBase
{
    private readonly IOptions<TemperatureApiConfig> _config;

    private string ApiGetResultOutputFormat => $"{{0:F{_config.Value.Outputs.Temperature.Decimals}}}";

    public TemperatureController(IOptions<TemperatureApiConfig> config)
    {
        _config = config;
    }

    [HttpGet("{cityName}")]
    public async Task<ActionResult<string>> Get([FromRoute] string cityName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_config.Value.Inputs.Cities.Contains(cityName, StringComparer.InvariantCultureIgnoreCase))
        {
            return NotFound($"City `{cityName}` not recognised");
        }

        decimal result = -1;

        string resultFormatted = string.Format(CultureInfo.InvariantCulture, ApiGetResultOutputFormat, result);
        return Ok(resultFormatted);
    }
}
