namespace WeatherServer.Api;

using WeatherServer.Api.TemperatureApi.Config;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        TemperatureApiConfig temperatureApiConfig = builder.Configuration.Get<TemperatureApiConfig>()
            ?? throw new TemperatureApiConfigException($"Failed to read the {nameof(TemperatureApiConfig)} app configuration tree");

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapOpenApi();
        app.UseHttpsRedirection();
        app.MapControllers();

        app.Run();
    }
}
