using FluentValidation;
using FluentValidation.AspNetCore;
using HelperAPI.Extensions;
using Serilog.Events;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System.Data;
using System.Reflection;

try
{
    // Setting SerilLog
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Map(
        evt => evt.Level,
        (level, wt) => wt.File(new Serilog.Formatting.Compact.CompactJsonFormatter(),@$"Logs\\{DateTime.Now.ToLongDateString()}\\{level}\\log-.txt", rollingInterval:RollingInterval.Day))
        .WriteTo.Console()
        .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddGlobalConfig();
    builder.Services.AddServices();
    builder.Services.AddRepositories();

    // Add  FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Add services to the container.
    builder.Services.AddEndpointsApiExplorer();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddSwaggerGen();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            builder =>
            {

                //you can configure your custom policy
                builder.AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
            });
    });

    // Add SerilLog
    builder.Host.UseSerilog();

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.Use(async (context, next) =>
    {
        // axios 無法獲得全部的header要加上這tag
        context.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
        await next();
    });

    app.UseCors();

    // 動態新增路由
    app.Routers();

    app.UseHttpsRedirection();

    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };


    app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateTime.Now.AddDays(index),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    });

    // Add SerilLog
    app.UseSerilogRequestLogging();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}




internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}