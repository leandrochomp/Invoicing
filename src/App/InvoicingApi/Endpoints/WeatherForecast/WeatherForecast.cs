using FastEndpoints;

namespace InvoicingApi.Endpoints.WeatherForecast;

public class GetWeatherForecastEndpoint : EndpointWithoutRequest<WeatherForecast[]>
{
    private static readonly string[] Summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public override void Configure()
    {
        Get("/weatherforecast");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get weather forecast data";
            s.Description = "Returns a collection of weather forecast data";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    Summaries[Random.Shared.Next(Summaries.Length)]
                ))
            .ToArray();
            
        await SendAsync(forecast, cancellation: ct);
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}