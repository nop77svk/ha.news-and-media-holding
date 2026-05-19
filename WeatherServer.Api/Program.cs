namespace WeatherServer.Api;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using WeatherServer.Api.TemperatureApi.Config;
using WeatherServer.Api.TemperatureApi.Services.ExternalWeatherApi;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        ConfigureConfiguration(builder);

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddHttpClient("ExternalWeatherApiClient", (serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetService<IOptions<TemperatureApi.Config.WeatherService.TemperatureApiWeatherServiceConfig>>()
                ?? throw new InvalidOperationException("Unable to retrieve the external weather API configuration.");

            httpClient.Timeout = TimeSpan.FromMilliseconds(options.Value.TimeoutMS);
        });

        builder.Services.AddScoped<IExternalWeatherApiClient>(serviceProvider =>
        {
            HttpClient httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("ExternalWeatherApiClient");
            var options = serviceProvider.GetRequiredService<IOptions<TemperatureApi.Config.WeatherService.TemperatureApiWeatherServiceConfig>>();
            return new GenericConfigurableWeatherApiClient(httpClient, options);
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapOpenApi();
        app.UseHttpsRedirection();
        app.MapControllers();

        app.Run();
    }

    private static void ConfigureConfiguration(WebApplicationBuilder builder)
    {
        IConfigurationSection appConfigBase = builder.Configuration.GetSection(nameof(TemperatureApiConfig));

        builder.Services.Configure<TemperatureApi.Config.Inputs.TemperatureApiInputs>(
            appConfigBase.GetSection(nameof(TemperatureApi.Config.Inputs)),
            binderOptions =>
            {
                binderOptions.ErrorOnUnknownConfiguration = true;
                binderOptions.BindNonPublicProperties = false;
            }
        );

        builder.Services.Configure<TemperatureApi.Config.WeatherService.TemperatureApiWeatherServiceConfig>(
            appConfigBase.GetSection(nameof(TemperatureApi.Config.WeatherService)),
            binderOptions =>
            {
                binderOptions.ErrorOnUnknownConfiguration = true;
                binderOptions.BindNonPublicProperties = false;
            }
        );

        builder.Services.Configure<TemperatureApi.Config.Outputs.TemperatureApiOutputs>(
            appConfigBase.GetSection(nameof(TemperatureApi.Config.Outputs)),
            binderOptions =>
            {
                binderOptions.ErrorOnUnknownConfiguration = true;
                binderOptions.BindNonPublicProperties = false;
            }
        );
    }
}
