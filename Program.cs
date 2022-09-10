using FluentValidation;
using FluentValidation.AspNetCore;
using HelperAPI.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Swashbuckle.AspNetCore.Swagger;
using System.Data;
using System.Reflection;

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

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}