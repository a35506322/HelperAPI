using HelperAPI.Helper;
using HelperAPI.Models;
using HelperAPI.Modules.Interfaces;
using InsuranceAgents.Domain.Helpers.UITC;
using IronOcr;
using Npoi.Mapper;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using Encoder = System.Drawing.Imaging.Encoder;

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

            app.MapGet("/parse_image", async (ScanHelper scanHelper, IronTesseract ironTesseract, CaptchaCrackedHelper captchaCrackedHelper) =>
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://test.yesgogogo.com/eMall/api/members/ValidateCode"))
                    {
                        request.Content = new StringContent("");
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = await httpClient.SendAsync(request);

                        ParseImageResponse? parseImageResponse;
                        if (response.IsSuccessStatusCode)
                        {
                            parseImageResponse = await response.Content.ReadFromJsonAsync<ParseImageResponse>();

                            if (parseImageResponse != null)
                            {
                                // 使用正則表達式擷取要轉換圖片的文字
                                Regex regex = new Regex(@"[^(data:image.+;base64,)].*");
                                MatchCollection matchStrings = regex.Matches(parseImageResponse.info.captcha);
                                string imgaeBase64 = matchStrings.FirstOrDefault().Value;

                                // 轉換圖片
                                Image image = scanHelper.Base64StringToImage(imgaeBase64);

                                Bitmap myBitmap = new Bitmap(image);

                                // Get an ImageCodecInfo object that represents the JPEG codec.
                                ImageCodecInfo myImageCodecInfo = scanHelper.GetEncoderInfo("image/gif");

                                // for the Quality parameter category.
                                Encoder myEncoder = Encoder.Quality;

                                // Create an EncoderParameters object.

                                // An EncoderParameters object has an array of EncoderParameter

                                // objects. In this case, there is only one

                                // EncoderParameter object in the array.
                                EncoderParameters myEncoderParameters = new EncoderParameters(1);

                                // 降躁圖片處理
                                captchaCrackedHelper.BmpSource = myBitmap;
                                captchaCrackedHelper.ConvertGrayByPixels();
                                captchaCrackedHelper.RemoteNoiseLineByPixels();
                                captchaCrackedHelper.RemoteNoisePointByPixels();

                                // Save the  a JPEG file with quality level 75.
                                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 75L);
                                myEncoderParameters.Param[0] = myEncoderParameter;
                                string imageName = DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "_verificationText.gif";
                                myBitmap.Save(imageName, myImageCodecInfo, myEncoderParameters);

                                // 解析圖片文字
                                var Result = ironTesseract.Read(myBitmap);
                                return Results.Ok(Result);
                            } 
                        }
                        return Results.StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
            });
        }
    }
}
