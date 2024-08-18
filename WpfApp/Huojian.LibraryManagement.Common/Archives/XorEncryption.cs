namespace Huojian.LibraryManagement.Common.Archives
{
    class XorEncryption : IEncryption
    {
        private byte[] _key;

        public XorEncryption()
        {
            _key = new byte[32];
            var rnd = new Random();
            rnd.NextBytes(_key);
        }

        public XorEncryption(byte[] metadata)
        {
            _key = metadata;
        }

        public byte[] Metadata => _key;

        public byte[] Decrypt(byte[] input)
        {
            return XOR(input);
        }

        public byte[] Encrypt(byte[] input)
        {
            return XOR(input);
        }

        public void Dispose()
        {
        }

        private byte[] XOR(byte[] input)
        {
            int len = input.Length;
            int keylen = _key.Length;
            for (int i = 0; i < len; i++)
                input[i] = (byte)(input[i] ^ _key[i % keylen]);
            return input;
        }
    }
}