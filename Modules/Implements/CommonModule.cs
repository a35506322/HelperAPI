using HelperAPI.Modules.Interfaces;
using InsuranceAgents.Domain.Helpers.UITC;
using Npoi.Mapper;
using System.Text;
using System.Text.Json;
using System.Web;

namespace HelperAPI.Modules.Implements
{
    public record ExcelForDecrypt(IFormFile excelFile, List<SheetInfo> sheetInfos);
    public record SheetInfo(string name, List<ColumnInfo> data);
    public record ColumnInfo(string columnName, string isDecrypt);

    public class CommonModule : IBaseModule
    {
        public void AddModuleRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/excel_for_decrypt", (HttpContext ctx, SecurityHelper securityHelper) =>
            {
                // 由於.net6 極簡API不支援FromForm標籤所以改從HttpContext讀取
                var columnInfos = JsonSerializer.Deserialize<List<SheetInfo>>(ctx.Request.Form["sheetInfos"].ToString());
                var files = ctx.Request.Form.Files;

                if (files.Count == 0 || columnInfos?.Count == 0)
                {
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
        }
    }
}
