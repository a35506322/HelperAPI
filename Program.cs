using InsuranceAgents.Domain.Helpers.UITC;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npoi.Mapper;
using NPOI.HPSF;
using NPOI.Util;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<SecurityHelper>();

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

// Configure the HTTP request pipeline.
app.Use(async (context, next) =>
{
    // axios 無法獲得全部的header要加上這tag
    context.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
    await next();
});

app.UseCors();

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

app.MapPost("/excel_for_decrypt", (HttpContext ctx, SecurityHelper securityHelper) =>
{
    // 由於.net6 極簡API不支援FromForm標籤所以改從HttpContext讀取
    var columnInfos = JsonSerializer.Deserialize<List<SheetInfo>>(ctx.Request.Form["sheetInfos"].ToString());
    var files = ctx.Request.Form.Files;

    if (files.Count == 0 || columnInfos?.Count == 0 ) {
        return Results.BadRequest("檔案有錯誤");
    }

    ExcelForDecrypt excelForDecrypt = new(files[0], columnInfos);
    
    var mapper = new Mapper(excelForDecrypt.excelFile.OpenReadStream());
    var sheetName = mapper.Workbook.GetSheetName(0);
    var sheet = mapper.Take<dynamic>(sheetName).ToList();
    // 一列一列讀取
    foreach (var row in sheet)
    {
        // 利用前端傳過來的columnData去尋找excel file5中的值,去判斷是否要解密
        foreach (var columnInfo in excelForDecrypt.sheetInfos[0].data)
        {
            // 映射屬性
            var propertyInfo = System.ComponentModel.TypeDescriptor.
                GetProperties((object)row.Value).
                Find(columnInfo.columnName, true);
            // null 套過
            if (propertyInfo != null)
            {
                /* 形態要判別一下不然會沒法SetValue
                   如果型態是String以及IsDecrypt == "Y"
                   即為解密
                 */
                if (propertyInfo.PropertyType.Name == "String" && columnInfo.isDecrypt == "Y")
                {
                    var fileValue = propertyInfo.GetValue((object)row.Value);
                    propertyInfo.SetValue((object)row.Value, securityHelper.DecryptData(fileValue.ToString()));
                }
                
            }
        }
    }

    // 匯出時候只取出Value值
    var returnSheet = sheet.Select(row => row.Value);
    var retunrMapper = new Mapper();
    // 解完密回傳回去
    using MemoryStream stream = new MemoryStream();
    retunrMapper.Save(stream, returnSheet, sheetName, overwrite: true, xlsx: true);
    /*
     HttpUtility.UrlEncode($"{sheetName}_解密.xlsx", Encoding.UTF8)
     使用Url加密 到前端在利用JS decodeURI()解掉
     不然中文傳去前端會亂碼
     */
    return Results.File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", HttpUtility.UrlEncode($"{sheetName}_解密.xlsx", Encoding.UTF8));
});

app.Run();

public record ExcelForDecrypt(IFormFile excelFile,List<SheetInfo> sheetInfos);
public record SheetInfo(string name, List<ColumnInfo> data);
public record ColumnInfo (string columnName,string isDecrypt);

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}