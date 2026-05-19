namespace WeatherServer.Api.TemperatureApi.Config.WeatherService;

public class ExternalWeatherApiCallException : Exception
{
    public ExternalWeatherApiCallException()
    {
    }

    public ExternalWeatherApiCallException(string message)
        : base(message)
    {
    }

    public ExternalWeatherApiCallException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
