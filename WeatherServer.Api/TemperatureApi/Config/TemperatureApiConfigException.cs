namespace WeatherServer.Api.TemperatureApi.Config;

using WeatherServer.Api.Infrastructure;

public class TemperatureApiConfigException : ConfigurationException
{
    public TemperatureApiConfigException()
    {
    }

    public TemperatureApiConfigException(string message)
        : base(message)
    {
    }

    public TemperatureApiConfigException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
