using FluentValidation;
using FluentValidation.AspNetCore;
using HelperAPI.Extensions;
using Serilog.Events;
using Serilog;
using System.Data;
using System.Reflection;
using ZymLabs.NSwag.FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using HelperAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddGlobalConfig();
    builder.Services.AddServices();
    builder.Services.AddRepositories();

    // Add JwtToken Valid
    builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
        options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

            // 一般我們都會驗證 Issuer
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetValue<string>("JwtSetting:Issuer"),

            // 通常不太需要驗證 Audience
            ValidateAudience = false,
            //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

            // 一般我們都會驗證 Token 的有效期間
            ValidateLifetime = true,

            // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
            ValidateIssuerSigningKey = false,

            // "1234567890123456" 應該從 IConfiguration 取得
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSetting:SignKey")))
        };
    });

    builder.Services.AddAuthorization();

    // Add  FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Register the Swagger services
    builder.Services.AddOpenApiDocument((settings, serviceProvider) =>
    {
        var fluentValidationSchemaProcessor = serviceProvider.CreateScope().ServiceProvider.GetService<FluentValidationSchemaProcessor>();

        // Add the fluent validations schema processor
        settings.SchemaProcessors.Add(fluentValidationSchemaProcessor);
    });
    // Add the FluentValidationSchemaProcessor as a scoped service
    builder.Services.AddScoped<FluentValidationSchemaProcessor>(provider =>
    {
        var validationRules = provider.GetService<IEnumerable<FluentValidationRule>>();
        var loggerFactory = provider.GetService<ILoggerFactory>();

        return new FluentValidationSchemaProcessor(provider, validationRules, loggerFactory);
    });

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

    // Add appsettings.json model
    builder.Services.AddOptions();
    builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JwtSetting"));

    // Add SerilLog
    builder.Host.UseSerilog();

    var app = builder.Build();

    // Register the Swagger generator and the Swagger UI middlewares
    app.UseOpenApi();
    app.UseSwaggerUi3();

    app.Use(async (context, next) =>
    {
        // axios 無法獲得全部的header要加上這tag
        context.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
        await next();
    });

    app.UseCors();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // 動態新增路由
    app.Routers();

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