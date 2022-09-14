using HelperAPI.Helper;
using HelperAPI.Models;
using HelperAPI.Modules.Implements;
using HelperAPI.Services.Interfaces;
using IronOcr;
using static HelperAPI.Modules.Implements.CommonModule;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;

namespace HelperAPI.Services.Implements
{
    public class CommonService : ICommonService
    {
        private readonly ScanHelper _scanHelper;
        private readonly IronTesseract _ironTesseract;

        public CommonService(ScanHelper scanHelper, IronTesseract ironTesseract)
        {
            this._scanHelper = scanHelper;
            this._ironTesseract = ironTesseract;
        }
        public async Task<CommonModule.SignInReqeust> getVaildTest()
        {
            SignInReqeust signInReqeust = new SignInReqeust();
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
                            signInReqeust.ValidTransactionId = parseImageResponse.info.validTransactionId;

                            // 使用正則表達式擷取要轉換圖片的文字
                            Regex regex = new Regex(@"[^(data:image.+;base64,)].*");
                            MatchCollection matchStrings = regex.Matches(parseImageResponse.info.captcha);
                            string imgaeBase64 = matchStrings.FirstOrDefault().Value;

                            // 轉換圖片
                            Image image = this._scanHelper.Base64StringToImage(imgaeBase64);

                            Bitmap myBitmap = new Bitmap(image);

                            CaptchaCrackedHelper captchaCrackedHelper = new CaptchaCrackedHelper();
                            // 降躁圖片處理
                            captchaCrackedHelper.BmpSource = myBitmap;
                            captchaCrackedHelper.ConvertGrayByPixels();
                            captchaCrackedHelper.RemoteNoiseLineByPixels();
                            captchaCrackedHelper.RemoteNoisePointByPixels();

                            // 解析圖片文字
                            var Result = await this._ironTesseract.ReadAsync(myBitmap);
                            signInReqeust.Captcha = Result.Text;
                        }
                    }
                }
            }
            return signInReqeust;
        }
    }
}
