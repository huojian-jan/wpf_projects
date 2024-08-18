using System.Security.Cryptography;
using System.Text;

namespace Huojian.LibraryManagement.Common.Utilities
{
    public class AESHelper
    {
        private byte[] _keyBytes;
        private byte[] _ivBytes;

        public AESHelper(string key, string iv)
        {
            _keyBytes = Convert.FromBase64String(key);
            _ivBytes = Encoding.UTF8.GetBytes(iv);
        }

        public string Encrypt(string plainText)
        {
            var cipherTextBytes = EncryptStringToBytes_Aes(plainText, _keyBytes, _ivBytes);
            return Convert.ToBase64String(cipherTextBytes);
        }

        public string Decrypt(string cipherText)
        {
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            return DecryptStringFromBytes_Aes(cipherTextBytes, _keyBytes, _ivBytes);
        }

        private byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;         // 服务端使用的是PKCS5

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }

    public class AESKeyHelper
    {
        private static readonly string _aesIv = "keosmnvbhdueyr2b";
        private static readonly Dictionary<int, string> _aesKeys = new Dictionary<int, string>
        {
            { 0, "1GJQae5adLt/JK8Fj4oIAA==" },
            { 1, "pO22DRcoQiho/omL8plzGQ==" },
            { 2, "dOK3FGctrQePCFYUkov+Yw==" },
            { 3, "yi3MQ+nU3qhsGWt6/iuGjw==" },
            { 4, "oedkwuT3udfkSVWjIb6zbg==" },
        };

        public static int RandomSecret()
        {
            return new Random().Next(0, 4);
        }

        public static string GetAesKeyBySecret(int secret)
        {
            if (_aesKeys.ContainsKey(secret))
                return _aesKeys[secret];
            return "";
        }

        public static string GetAesIv()
        {
            return _aesIv;
        }
    }
}
