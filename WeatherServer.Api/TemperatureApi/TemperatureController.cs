namespace WeatherServer.Api.TemperatureApi;

using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using WeatherServer.Api.Infrastructure;

[ApiController]
[Route(ApiConstants.TemperatureApiRoot)]
public class TemperatureController : ControllerBase
{
    private readonly IOptions<Config.Outputs.TemperatureApiOutputs> _outputsConfig;
    private readonly IOptions<Config.Inputs.TemperatureApiInputs> _inputsConfig;

    private string ApiGetResultOutputFormat => $"{{0:F{_outputsConfig.Value.Temperature.Decimals}}}";

    public TemperatureController(IOptions<Config.Outputs.TemperatureApiOutputs> outputsConfig, IOptions<Config.Inputs.TemperatureApiInputs> inputsConfig)
    {
        _outputsConfig = outputsConfig;
        _inputsConfig = inputsConfig;
    }

    [HttpGet("{cityName}")]
    public async Task<ActionResult<string>> GetTemperature([FromRoute] string cityName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_inputsConfig.Value.Cities.Contains(cityName, StringComparer.InvariantCultureIgnoreCase))
        {
            return NotFound($"City `{cityName}` not recognised");
        }

        decimal result = -1;

        string resultFormatted = string.Format(CultureInfo.InvariantCulture, ApiGetResultOutputFormat, result);
        return Ok(resultFormatted);
    }
}
