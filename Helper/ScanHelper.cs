using System.Drawing;
using System.Drawing.Imaging;

namespace HelperAPI.Helper
{
    public  class ScanHelper
    {
        /// <summary>
        /// base 64字串格式的圖片轉成Image物件
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public Image Base64StringToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] Buffer = Convert.FromBase64String(base64String);

            byte[] data = null;
            Image oImage = null;
            MemoryStream oMemoryStream = null;
            Bitmap oBitmap = null;
            //建立副本
            data = (byte[])Buffer.Clone();
            try
            {
                oMemoryStream = new MemoryStream(data);
                //設定資料流位置
                oMemoryStream.Position = 0;
                oImage = System.Drawing.Image.FromStream(oMemoryStream);
                //建立副本
                oBitmap = new Bitmap(oImage);
            }
            catch
            {
                throw;
            }
            finally
            {
                oMemoryStream.Close();
                oMemoryStream.Dispose();
                oMemoryStream = null;
            }
            //return oImage;
            return oBitmap;
        }

        public ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}
