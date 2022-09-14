using System.Drawing;

namespace HelperAPI.Helper
{
    public class CaptchaCrackedHelper
    {
        /// <summary>
        /// 存放來源圖檔
        /// </summary>
        public Bitmap BmpSource { get; set; }

        /// <summary>
        /// 將每點像素色彩轉換成灰階值
        /// </summary>
        public void ConvertGrayByPixels()
        {
            for (int i = 0; i < BmpSource.Height; i++)
                for (int j = 0; j < BmpSource.Width; j++)
                {
                    int grayValue = GetGrayValue(BmpSource.GetPixel(j, i));
                    BmpSource.SetPixel(j, i, Color.FromArgb(grayValue, grayValue, grayValue));
                }
        }

        /// <summary>
        /// 計算灰階值
        /// </summary>
        /// <param name="pColor">color-像素色彩</param>
        /// <returns></returns>
        private int GetGrayValue(Color pColor)
        {
            return Convert.ToInt32(pColor.R * 0.299 + pColor.G * 0.587 + pColor.B * 0.114); // 灰階公式
        }

        /// <summary>
        /// 噪音線處理
        /// </summary>
        public void RemoteNoiseLineByPixels()
        {
            for (int i = 0; i < BmpSource.Height; i++)
                for (int j = 0; j < BmpSource.Width; j++)
                {
                    int R = BmpSource.GetPixel(j, i).R;
                    int G = BmpSource.GetPixel(j, i).G;
                    int B = BmpSource.GetPixel(j, i).B;
                    if (R <= 255 && R >= 160)
                        BmpSource.SetPixel(j, i, Color.FromArgb(255, 255, 255));

                    if (R == 0 || B == 0 || G == 0)
                        BmpSource.SetPixel(j, i, Color.FromArgb(255, 255, 255));

                }
        }

        /// <summary>
        /// 噪音點處理
        /// </summary>
        public void RemoteNoisePointByPixels()
        {
            List<NoisePoint> points = new List<NoisePoint>();

            for (int k = 0; k < 5; k++)
            {
                for (int i = 0; i < BmpSource.Height; i++)
                    for (int j = 0; j < BmpSource.Width; j++)
                    {
                        int flag = 0;
                        int garyVal = 255;
                        // 檢查上相鄰像素
                        if (i - 1 > 0 && BmpSource.GetPixel(j, i - 1).R != garyVal) flag++;
                        if (i + 1 < BmpSource.Height && BmpSource.GetPixel(j, i + 1).R != garyVal) flag++;
                        if (j - 1 > 0 && BmpSource.GetPixel(j - 1, i).R != garyVal) flag++;
                        if (j + 1 < BmpSource.Width && BmpSource.GetPixel(j + 1, i).R != garyVal) flag++;
                        if (i - 1 > 0 && j - 1 > 0 && BmpSource.GetPixel(j - 1, i - 1).R != garyVal) flag++;
                        if (i + 1 < BmpSource.Height && j - 1 > 0 && BmpSource.GetPixel(j - 1, i + 1).R != garyVal) flag++;
                        if (i - 1 > 0 && j + 1 < BmpSource.Width && BmpSource.GetPixel(j + 1, i - 1).R != garyVal) flag++;
                        if (i + 1 < BmpSource.Height && j + 1 < BmpSource.Width && BmpSource.GetPixel(j + 1, i + 1).R != garyVal) flag++;

                        if (flag < 3)
                            points.Add(new NoisePoint() { X = j, Y = i });
                    }
                foreach (NoisePoint point in points)
                    BmpSource.SetPixel(point.X, point.Y, Color.FromArgb(255, 255, 255));

            }
        }

        public class NoisePoint
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
