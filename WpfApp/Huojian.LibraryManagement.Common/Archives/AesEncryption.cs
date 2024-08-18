using System.Security.Cryptography;

namespace Huojian.LibraryManagement.Common.Archives
{
    class AesEncryption : IEncryption
    {
        //private static readonly byte[] _iv = new byte[] { 189, 60, 255, 159, 226, 24,
        //        244, 230, 227, 160, 151, 210, 67,
        //        76, 244, 154 };

        //private static readonly byte[] _key = new byte[] { 7, 10, 160, 70, 115, 129,
        //        180, 211, 148, 179, 229, 203, 154,
        //        15, 207, 17, 108, 128, 119, 254, 155,
        //        194, 161, 126, 90, 226, 219, 13, 188,
        //        246, 74, 55 };

        private readonly Rijndael _aesAlg;
        private readonly ICryptoTransform _decryptor;
        private readonly ICryptoTransform _encryptor;

        public AesEncryption()
            : this(null)
        { }

        public AesEncryption(byte[] metadata)
        {
            _aesAlg = Rijndael.Create();
            if (metadata == null)
            {
                Metadata = new byte[1 + _aesAlg.IV.Length + 1 + _aesAlg.Key.Length];
                Metadata[0] = (byte)_aesAlg.IV.Length;
                Array.Copy(_aesAlg.IV, 0, Metadata, 1, _aesAlg.IV.Length);
                Metadata[1 + _aesAlg.IV.Length] = (byte)_aesAlg.Key.Length;
                Array.Copy(_aesAlg.Key, 0, Metadata, 1 + _aesAlg.IV.Length + 1, _aesAlg.Key.Length);
            }
            else
            {
                Metadata = metadata;
                var ivLength = metadata[0];
                var iv = new byte[ivLength];
                Array.Copy(metadata, 1, iv, 0, ivLength);
                var keyLength = metadata[1 + ivLength];
                var key = new byte[keyLength];
                Array.Copy(metadata, 1 + ivLength + 1, key, 0, keyLength);
                _aesAlg.IV = iv;
                _aesAlg.Key = key;
            }
            _decryptor = _aesAlg.CreateDecryptor();
            _encryptor = _aesAlg.CreateEncryptor();
        }

        public byte[] Metadata { get; }

        public byte[] Decrypt(byte[] input)
        {
            using (var msDecrypt = new MemoryStream(input))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, _decryptor, CryptoStreamMode.Read))
                {
                    return ReadFully(csDecrypt);
                }
            }
        }

        public byte[] Encrypt(byte[] input)
        {
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, _encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(input, 0, input.Length);
                    csEncrypt.FlushFinalBlock();
                    return msEncrypt.ToArray();
                }
            }
        }

        public void Dispose()
        {
            if (_decryptor != null)
                _decryptor.Dispose();
            if (_encryptor != null)
                _encryptor.Dispose();
            if (_aesAlg != null)
                _aesAlg.Dispose();
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

    }
}
