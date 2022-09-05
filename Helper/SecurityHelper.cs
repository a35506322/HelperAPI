using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;

namespace InsuranceAgents.Domain.Helpers.UITC
{
    public class SecurityHelper
    {
        private readonly byte[] _keyConn = Encoding.Unicode.GetBytes("聯邦網通加密鍵值Key     ");
        private readonly byte[] _ivConn = Encoding.Unicode.GetBytes("聯邦網通加密鍵值IV      ");
        private readonly byte[] _keyData = Encoding.Unicode.GetBytes("聯邦網通KeyData     ");
        private readonly byte[] _ivData = Encoding.Unicode.GetBytes("聯邦網通IVData      ");

        public string EncryptConn(string data) => Encrypt(data, _keyConn, _ivConn);
        public string DecryptConn(string data) => Decrypt(data, _keyConn, _ivConn);

        public string EncryptData(string data) => Encrypt(data, _keyData, _ivData);

        public string DecryptData(string data) => Decrypt(data, _keyData, _ivData);

        private static string Encrypt(string data, byte[] key, byte[] iv)
        {
            var inputBytes = Encoding.UTF8.GetBytes(data);
            var engine = new RijndaelEngine(256);
            var blockCipher = new CbcBlockCipher(engine);
            var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
            var keyParam = new KeyParameter(key);
            var keyParamWithIv = new ParametersWithIV(keyParam, iv, 0, 32);
            cipher.Init(true, keyParamWithIv);
            var outputBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
            var length = cipher.ProcessBytes(inputBytes, outputBytes, 0);
            cipher.DoFinal(outputBytes, length); //Do the final block
            return Convert.ToBase64String(outputBytes);
        }
        private static string Decrypt(string data, byte[] key, byte[] iv)
        {
            try
            {
                var inputBytes = Convert.FromBase64String(data);
                var engine = new RijndaelEngine(256);
                var blockCipher = new CbcBlockCipher(engine);
                CipherUtilities.GetCipher("AES/CTR/NoPadding");
                IBufferedCipher cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
                var keyParam = new KeyParameter(key);
                var keyParamWithIv = new ParametersWithIV(keyParam, iv, 0, 32);
                cipher.Init(false, keyParamWithIv);
                var outputBytes = new byte[cipher.GetOutputSize(inputBytes.Length)];
                var length = cipher.ProcessBytes(inputBytes, outputBytes, 0);
                cipher.DoFinal(outputBytes, length); //Do the final block
                return Encoding.UTF8.GetString(outputBytes).Split('\0')[0];
            }
            catch (Exception ex)
            {
                return data;
            }

        }

        /// <summary>
        /// For password
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string GetMd5Hash(string input)
        {
            // Create a new instance of the MD5 object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            int i;
            for (i = 0; i <= data.Length - 1; i++)
                sBuilder.Append(data[i].ToString("x2"));

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
