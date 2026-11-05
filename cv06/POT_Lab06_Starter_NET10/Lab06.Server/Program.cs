var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
// U1: registrujte validaciu a spolocne ulozisko knih.
// U4: po instalacii balicka zaregistrujte gRPC.
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Lab06"));
}
app.UseHttpsRedirection();
app.MapGet("/", () => "Lab06 server bezi. Otvorte /swagger.");
app.MapGet("/weatherforecast", () => new[]
{
    new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 15, "Ukazka")
});
// U1 az U3: doplnte endpointy /api/books podla navodu.
// U4: namapujte vlastnu gRPC sluzbu.
app.Run();
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);
