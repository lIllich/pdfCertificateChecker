using System.Security.Cryptography;
using System.Text;

namespace EncryptionDecryptionUsingSymmetricKey
{
    public class AesEncryption
    {
        private static byte[] encryptionKey = Encoding.ASCII.GetBytes("iprojektiprojekt");
        private static byte[] initializationVector = Encoding.ASCII.GetBytes("0100011101100101");

        public static string DecryptString(string cipherText)
        {
            try
            {
                byte[] encryptedBytes = FromHex(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = encryptionKey;
                    aes.IV = initializationVector;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    string decryptedStr = Encoding.UTF8.GetString(decryptedBytes);

                    return decryptedStr;
                }
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public static string EncryptString(string plainText)
        {
            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = encryptionKey;
                    aes.IV = initializationVector;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    string encryptedStr = ToHex(encryptedBytes);

                    return encryptedStr;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static string ToHex(byte[] bytes)
        {
            try
            {
                StringBuilder sb = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        static byte[] FromHex(string hex)
        {
            try
            {
                byte[] bytes = new byte[hex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return bytes;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
