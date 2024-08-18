namespace Huojian.LibraryManagement.Common.Archives
{
    interface IEncryption : IDisposable
    {
        byte[] Metadata { get; }

        byte[] Encrypt(byte[] input);

        byte[] Decrypt(byte[] input);
    }
}