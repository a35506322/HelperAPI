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
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;
using Spire.OCR;
using GodPay_CMS.Common.Helpers.Decipher;

namespace HelperAPI.Services.Implements
{
    public class CommonService : ICommonService
    {
        private readonly ScanHelper _scanHelper;
        private readonly IronTesseract _ironTesseract;
        private readonly OcrScanner _ocrScanner;
        private readonly CaptchaCrackedHelper _captchaCrackedHelper;
        private readonly DecipherHelper _decipherHelper;

        public CommonService(ScanHelper scanHelper,
            IronTesseract ironTesseract,
            OcrScanner ocrScanner,
            CaptchaCrackedHelper captchaCrackedHelper,
            DecipherHelper decipherHelper)
        {
            this._scanHelper = scanHelper;
            this._ironTesseract = ironTesseract;
            this._ocrScanner = ocrScanner;
            this._captchaCrackedHelper = captchaCrackedHelper;
            this._decipherHelper = decipherHelper;
        }

        public async Task<string> Decipher(DecipherRequest decipherRequest)
        {
            switch (decipherRequest.decipherCommandEnum)
            { 
                case Enums.DecipherCommandEnum.DataEncryptorAES:
                    return  this._decipherHelper.DataEncryptorAES(decipherRequest.data);

                case Enums.DecipherCommandEnum.DataDecryptorAES:
                    return this._decipherHelper.DataDecryptorAES(decipherRequest.data);

                case Enums.DecipherCommandEnum.ConnEncryptorAES:
                    return this._decipherHelper.ConnEncryptorAES(decipherRequest.data);

                case Enums.DecipherCommandEnum.ConnDecryptorAES:

                    return this._decipherHelper.ConnDecryptorAES(decipherRequest.data);
                default:
                    throw new ArgumentException("無此操作");
            }
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

                            // 降躁圖片處理
                            this._captchaCrackedHelper.BmpSource = myBitmap;
                            this._captchaCrackedHelper.ConvertGrayByPixels();
                            this._captchaCrackedHelper.RemoteNoiseLineByPixels();
                            this._captchaCrackedHelper.RemoteNoisePointByPixels();

                            // Save the  a Bit
                            string imageName = DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "_verificationText.bmp";
                            myBitmap.Save(imageName);

                            // 讀出檔案
                            this._ocrScanner.Scan(imageName);
                            string scanText = this._ocrScanner.Text.ToString();

                            // 刪除檔案
                            File.Delete(imageName);

                            // 使用正則表達式擷取要掃描後的文字
                            Regex regex2 = new Regex(@"^[\w\d]{4}");
                            MatchCollection matchStrings2 = regex2.Matches(scanText);
                            string scanTextByRex = matchStrings2.FirstOrDefault().Value;
                            signInReqeust.Captcha = scanTextByRex;

                            // 解析圖片文字
                            //var Result = await this._ironTesseract.ReadAsync(myBitmap);
                            //signInReqeust.Captcha = Result.Text;
                        }
                    }
                }
            }
            return signInReqeust;
        }

        public async Task<string> ImageOcr(IFormFile formFile, bool IsGraphicalVerification)
        {
            // file 轉 bitmap
            Bitmap myBitmap;
            using (var memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);
                using (var img = Image.FromStream(memoryStream))
                {
                    myBitmap = new Bitmap(img);
                }
            }

            // 是否是圖形驗證碼
            if (IsGraphicalVerification)
            {
                // 降躁圖片處理
                this._captchaCrackedHelper.BmpSource = myBitmap;
                this._captchaCrackedHelper.ConvertGrayByPixels();
                this._captchaCrackedHelper.RemoteNoiseLineByPixels();
                this._captchaCrackedHelper.RemoteNoisePointByPixels();
            }

            // Save the  a Bit
            string imageName = DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "_verificationText.bmp";
            myBitmap.Save(imageName);
;
            // 掃描圖片
            this._ocrScanner.Scan(imageName);
            string scanText = this._ocrScanner.Text.ToString();

            // 刪除檔案
            File.Delete(imageName);

            // 使用正則表達式擷取要掃描後的文字
            Regex regex2 = new Regex(@"^[\w\d]{4}");
            MatchCollection matchStrings2 = regex2.Matches(scanText);
            string scanTextByRex = matchStrings2.FirstOrDefault().Value;
            return scanTextByRex;

            // 解析圖片文字
            //var Result = await this._ironTesseract.ReadAsync(myBitmap);
            //return Result;
        }
    }
}
