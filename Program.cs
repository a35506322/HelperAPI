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
    // axios �L�k��o������header�n�[�W�otag
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
    // �ѩ�.net6 ��²API���䴩FromForm���ҩҥH��qHttpContextŪ��
    var columnInfos = JsonSerializer.Deserialize<List<SheetInfo>>(ctx.Request.Form["sheetInfos"].ToString());
    var files = ctx.Request.Form.Files;

    if (files.Count == 0 || columnInfos?.Count == 0 ) {
        return Results.BadRequest("�ɮצ����~");
    }

    ExcelForDecrypt excelForDecrypt = new(files[0], columnInfos);
    
    var mapper = new Mapper(excelForDecrypt.excelFile.OpenReadStream());
    var sheetName = mapper.Workbook.GetSheetName(0);
    var sheet = mapper.Take<dynamic>(sheetName).ToList();
    // �@�C�@�CŪ��
    foreach (var row in sheet)
    {
        // �Q�Ϋe�ݶǹL�Ӫ�columnData�h�M��excel file5������,�h�P�_�O�_�n�ѱK
        foreach (var columnInfo in excelForDecrypt.sheetInfos[0].data)
        {
            // �M�g�ݩ�
            var propertyInfo = System.ComponentModel.TypeDescriptor.
                GetProperties((object)row.Value).
                Find(columnInfo.columnName, true);
            // null �M�L
            if (propertyInfo != null)
            {
                /* �κA�n�P�O�@�U���M�|�S�kSetValue
                   �p�G���A�OString�H��IsDecrypt == "Y"
                   �Y���ѱK
                 */
                if (propertyInfo.PropertyType.Name == "String" && columnInfo.isDecrypt == "Y")
                {
                    var fileValue = propertyInfo.GetValue((object)row.Value);
                    propertyInfo.SetValue((object)row.Value, securityHelper.DecryptData(fileValue.ToString()));
                }
                
            }
        }
    }

    // �ץX�ɭԥu���XValue��
    var returnSheet = sheet.Select(row => row.Value);
    var retunrMapper = new Mapper();
    // �ѧ��K�^�Ǧ^�h
    using MemoryStream stream = new MemoryStream();
    retunrMapper.Save(stream, returnSheet, sheetName, overwrite: true, xlsx: true);
    /*
     HttpUtility.UrlEncode($"{sheetName}_�ѱK.xlsx", Encoding.UTF8)
     �ϥ�Url�[�K ��e�ݦb�Q��JS decodeURI()�ѱ�
     ���M����ǥh�e�ݷ|�ýX
     */
    return Results.File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", HttpUtility.UrlEncode($"{sheetName}_�ѱK.xlsx", Encoding.UTF8));
});

app.Run();

public record ExcelForDecrypt(IFormFile excelFile,List<SheetInfo> sheetInfos);
public record SheetInfo(string name, List<ColumnInfo> data);
public record ColumnInfo (string columnName,string isDecrypt);

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}