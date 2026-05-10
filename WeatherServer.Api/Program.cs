namespace WeatherServer.Api;

using WeatherServer.Api.TemperatureApi.Config;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<TemperatureApiConfig>(
            builder.Configuration.GetSection(nameof(TemperatureApiConfig)),
            binderOptions =>
            {
                binderOptions.ErrorOnUnknownConfiguration = true;
                binderOptions.BindNonPublicProperties = false;
            }
        );

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
