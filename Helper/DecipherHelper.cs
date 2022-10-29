using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace GodPay_CMS.Common.Helpers.Decipher
{
    public class DecipherHelper
    {
        private string result;
        private byte[] KeyConn;

        private byte[] IVConn;

        private byte[] KeyData;

        private byte[] IVData;

        private string publicXml;

        private string privateXml;

        public DecipherHelper()
        {
            this.KeyConn = Encoding.Unicode.GetBytes("聯邦網通加密鍵值Key     ");
            this.IVConn = Encoding.Unicode.GetBytes("聯邦網通加密鍵值IV      ");
            this.KeyData = Encoding.Unicode.GetBytes("聯邦網通KeyData     ");
            this.IVData = Encoding.Unicode.GetBytes("聯邦網通IVData      ");
            this.publicXml = "<RSAKeyValue><Modulus>wqr9lxwojtrQm7btlbkEo/+ATOngVnsbTlVmvmNeJe/+YhblkfAtq4oCf1yGqXPfUrE0d/dOUpfQbOOxvRCEzHxxisqxlcUHj5AUkzv6ZA1H+vst8YGra4REPlkY0Hz9IQQpSYigEspvV2LFlK0xqBGZWMcHtO5roBr8tCxxJSE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            this.privateXml = "<RSAKeyValue><Modulus>wqr9lxwojtrQm7btlbkEo/+ATOngVnsbTlVmvmNeJe/+YhblkfAtq4oCf1yGqXPfUrE0d/dOUpfQbOOxvRCEzHxxisqxlcUHj5AUkzv6ZA1H+vst8YGra4REPlkY0Hz9IQQpSYigEspvV2LFlK0xqBGZWMcHtO5roBr8tCxxJSE=</Modulus><Exponent>AQAB</Exponent><P>4BCc11wsn3Nv1h2JU8q5XVmpwGIlFmY+ABkjX/ywXPRR9foTkqGNsJwWzgS99VYp5a7s3LQGVi9vI4Uh+Jh3hw==</P><Q>3mnHrKV0CYA64RkU0yw08TCM5MPagJabqtIZl3sh9PUzY//oLv3S6NqFBR+EAig0dXzIp9iUG3rXMdPbPxhYFw==</Q><DP>sPWN2Sxr9ZZm2hTDs5Ck6wv4W/9nSRSAnPU9kf5wj0lKPdk+ggzjaXstK5JlMBDX0BVh7kCjzIdz3/qyRLKtmw==</DP><DQ>B7JpcpQXO+zwHLIdgmFZQ6+GcLRGb4TGxlaXBCMCvdNuf9tvUZD/J4fIarD4hIqXpik3WHeqSHkr0VGfmMNi6Q==</DQ><InverseQ>EMiMWWpZ/+SvAorbiJHHP3MJsZq/pX3EdEgM07N6FfActOIJU0cTg8to6s2s7bwaJOV0hww0oDBXfS6EyyLYMw==</InverseQ><D>nRubJoWXRhPbIJD2FkwILsNaLLjkUWdxljrefPF9XmjeiROpm6qXcUYk1d064S+fIQHbMqbpE0dq8zagj9HxGh9OhBmwkMVkWOGd4NpgvHqAdSBpsRiSYyti5faBFCB35r3nB4KAUU+srtMwg6wZUnhk68oPyES/V1Bm3JWTVHE=</D></RSAKeyValue>";
        }

        public string DataEncryptorAES(string Data)
        {
            result = EncryptorAES(Data, this.KeyData, this.IVData);
            return result;
        }
        public string DataDecryptorAES(string Data)
        {

            result = DecryptorAES(Data, this.KeyData, this.IVData);

            return result;
        }
        public string ConnEncryptorAES(string Data)
        {
            result = EncryptorAES(Data, this.KeyConn, this.IVConn);
            return result;
        }
        public string ConnDecryptorAES(string Data)
        {

            result = DecryptorAES(Data, this.KeyConn, this.IVConn);

            return result;
        }
        public string EncryptorRSA(string Data)
        {
            result = EncryptRSA(Data, this.publicXml);
            return result;
        }
        public string DecryptorRSA(string Data)
        {
            result = DecryptRSA(Data, this.privateXml);
            return result;
        }
        public string MD5(string Data)
        {
            using MD5 csp = System.Security.Cryptography.MD5.Create();
            var result = BitConverter.ToString(csp.ComputeHash(Encoding.UTF8.GetBytes(Data))).Replace("-", string.Empty);
            return result;
        }
        public string SHA1(string Data)
        {
            using SHA1 csp = System.Security.Cryptography.SHA1.Create();
            var result = BitConverter.ToString(csp.ComputeHash(Encoding.UTF8.GetBytes(Data))).Replace("-", string.Empty);
            return result;
        }
        public string SHA256(string Data)
        {
            using SHA256 csp = System.Security.Cryptography.SHA256.Create();
            var result = BitConverter.ToString(csp.ComputeHash(Encoding.UTF8.GetBytes(Data))).Replace("-", string.Empty);
            return result;
        }
        public string SHA384(string Data)
        {
            using SHA384 csp = System.Security.Cryptography.SHA384.Create();
            var result = BitConverter.ToString(csp.ComputeHash(Encoding.UTF8.GetBytes(Data))).Replace("-", string.Empty);
            return result;
        }
        public string SHA512(string Data)
        {
            using SHA512 csp = System.Security.Cryptography.SHA512.Create();
            var result = BitConverter.ToString(csp.ComputeHash(Encoding.UTF8.GetBytes(Data))).Replace("-", string.Empty);
            return result;
        }
        private static string EncryptorAES(string Data, byte[] Key, byte[] IV)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(Data);
            var engine = new RijndaelEngine(256);
            var blockCipher = new CbcBlockCipher(engine);
            var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
            var keyParam = new KeyParameter(Key);
            var keyParamWithIV = new ParametersWithIV(keyParam, IV, 0, 32);

            cipher.Init(true, keyParamWithIV);
            byte[] outputBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
            int length = cipher.ProcessBytes(inputBytes, outputBytes, 0);
            cipher.DoFinal(outputBytes, length); //Do the final block
            return Convert.ToBase64String(outputBytes);
        }
        private static string DecryptorAES(string Data, byte[] Key, byte[] IV)
        {
            byte[] inputBytes = System.Convert.FromBase64String(Data);
            var engine = new RijndaelEngine(256);
            var blockCipher = new CbcBlockCipher(engine);
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
            cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
            var keyParam = new KeyParameter(Key);
            var keyParamWithIV = new ParametersWithIV(keyParam, IV, 0, 32);
            cipher.Init(false, keyParamWithIV);
            byte[] outputBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
            var length = cipher.ProcessBytes(inputBytes, outputBytes, 0);
            cipher.DoFinal(outputBytes, length); //Do the final block
            string[] words = Encoding.UTF8.GetString(outputBytes).Split('\0');
            return words[0];
        }
        private static string EncryptRSA(string Data, string Key)
        {

            using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(Key);
            byte[] bytes = Encoding.Default.GetBytes(Data);
            var result = BitConverter.ToString(rsa.Encrypt(bytes, false)).Replace("-", string.Empty);

            return result;

        }
        private static string DecryptRSA(string Data, string Key)
        {
            using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(Key);
            byte[] array = new byte[Data.Length / 2 - 1 + 1];
            int num = 0;
            int num2 = Data.Length / 2 - 1;
            int num3 = 0;
            while (true)
            {
                if (num3 > num2)
                {
                    break;
                }
                array[num3] = byte.Parse(Data[num].ToString() + Data[num + 1].ToString(), NumberStyles.HexNumber);
                num += 2;
                num3++;
            }
            var result = Encoding.Default.GetString(rsa.Decrypt(array, false));
            return result;
        }
    }
}
