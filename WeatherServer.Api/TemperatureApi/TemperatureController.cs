namespace WeatherServer.Api.TemperatureApi;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using WeatherServer.Api.Infrastructure;
using WeatherServer.Api.TemperatureApi.Config;

[ApiController]
[Route(ApiConstants.TemperatureApiRoot)]
public class TemperatureController : ControllerBase
{
    private readonly IOptions<TemperatureApiConfig> _config;

    public TemperatureController(IOptions<TemperatureApiConfig> config)
    {
        _config = config;
    }

    [HttpGet("{cityName}")]
    public ActionResult<decimal> Get([FromRoute] string cityName)
    {
        if (!_config.Value.Inputs.Cities.Contains(cityName, StringComparer.InvariantCultureIgnoreCase))
        {
            return NotFound($"City `{cityName}` not recognised");
        }

        return Ok(-1);
    }
}
