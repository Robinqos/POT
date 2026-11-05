using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => "Hello World!");

// 1) Route parameter: /greet/John
//app.MapGet("/greet/{name}", (string name) =>
//{
//    return Results.Ok($"Ahoj, {name}!");
//});

// 2) Query parameters: /add?x=3&y=4
app.MapGet("/add", (int x, int y) =>
{
    return Results.Ok(new { x, y, sum = x + y });
});

// 3) Optional query parameter with default: /hello or /hello?name=Eva
app.MapGet("/hello", (string? name) =>
{
    name ??= "neznámy";
    return $"Ahoj, {name}!";
});

// 4) POST with model binding from JSON body

app.MapPost("/person", ([FromBody] Person person) =>
{
    if (person is null) return Results.BadRequest("Chıba telo poiadavky (Person).");
    if (person.Age < 0) return Results.BadRequest("Vek musí by nezáporné èíslo.");

    // V reálnej aplikácii by ste uloili person do DB
    return Results.Created($"/person/{person.FirstName}", person);
});

// 5) Injected service (ILogger) and CancellationToken example
app.MapGet("/time", (ILogger<Program> logger, CancellationToken ct) =>
{
    logger.LogInformation("Poiadavka na /time prijatá.");
    // CancellationToken je u pripravenı a bude signalizovanı pri zrušení poiadavky klientom
    return Results.Ok(new { utcNow = DateTime.UtcNow, cancelled = ct.IsCancellationRequested });
});

// 6) Async endpoint that respects CancellationToken
app.MapGet("/wait", async (CancellationToken ct) =>
{
    // Simulujeme prácu, ktorú mono zruši
    await Task.Delay(5000, ct);
    return Results.Ok("Hotovo po èakaní 5s (ak to nebolo zrušené).");
});

// Route parameter with simple validation: /greet/John
app.MapGet("/greet/{name}", (string name) =>
{
    if (string.IsNullOrWhiteSpace(name))
        return Results.BadRequest(new { error = "Meno nesmie by prázdne." });

    if (name.Length > 50)
        return Results.BadRequest(new { error = "Meno je príliš dlhé (max 50 znakov)." });

    return Results.Ok($"Ahoj, {name}!");
});

// POST endpoint s validáciou DataAnnotations: /person2
app.MapPost("/person2", ([FromBody] PersonValidated? person) =>
{
    if (person is null)
        return Results.BadRequest(new { error = "Chıba telo poiadavky (Person)." });

    // Manual DataAnnotations validation
    var validationResults = new List<ValidationResult>();
    var ctx = new ValidationContext(person);
    if (!Validator.TryValidateObject(person, ctx, validationResults, validateAllProperties: true))
    {
        var errors = validationResults
            .SelectMany(r => (r.MemberNames?.Any() == true ? r.MemberNames : new[] { string.Empty })
                .Select(m => new { Member = m, r.ErrorMessage }))
            .GroupBy(x => x.Member)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors);
    }

    // V reálnej aplikácii by ste uloili person do DB
    return Results.Created($"/person/{person.FirstName}", person);
});

app.MapPost("/person3", ([FromBody] Person? person) =>
    {
        // handler sa môe teraz sústredi len na spracovanie platného modelu
        return Results.Created($"/person/{person!.FirstName}", person);
    })
    .AddEndpointFilter<ValidationFilter<Person>>(); // aplikovanie filtra


app.Run();

public record Person(string FirstName, string LastName, int Age);



public record PersonValidated(
    [property: Required(ErrorMessage = "FirstName je povinné.")]
    [property: StringLength(100, MinimumLength = 1, ErrorMessage = "FirstName musí ma 1-100 znakov.")]
    string FirstName,

    [property: Required(ErrorMessage = "LastName je povinné.")]
    [property: StringLength(100, MinimumLength = 1, ErrorMessage = "LastName musí ma 1-100 znakov.")]
    string LastName,

    [property: Range(0, 150, ErrorMessage = "Age musí by medzi 0 a 150.")]
    int Age
);


public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // nájdeme prvı argument typu T (napr. Person)
        var arg = context.Arguments.FirstOrDefault(a => a is T);

        // ak chıba (napr. telo requestu bolo null), vrátime BadRequest
        if (arg is null)
        {
            return Results.BadRequest(new { error = "Chıba telo poiadavky (model)." });
        }

        // DataAnnotations validácia
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(arg);
        if (!Validator.TryValidateObject(arg, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = validationResults
                .SelectMany(r => (r.MemberNames?.Any() == true ? r.MemberNames : new[] { string.Empty })
                    .Select(m => new { Member = m, r.ErrorMessage }))
                .GroupBy(x => x.Member)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }

        // všetko OK — pokraèujeme k handleru
        return await next(context);
    }
}