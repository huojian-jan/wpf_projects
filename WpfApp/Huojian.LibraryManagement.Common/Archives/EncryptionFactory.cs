namespace Huojian.LibraryManagement.Common.Archives;

class EncryptionFactory
{
    public static IEncryption CreateEncryption(EncryptionKind kind)
    {
        switch (kind)
        {
            case EncryptionKind.Xor:
                return new XorEncryption();
            case EncryptionKind.Aes:
                return new AesEncryption();
            default:
                throw new NotImplementedException();
        }
    }

    public static IEncryption CreateEncryption(EncryptionKind kind, byte[] metadata)
    {
        switch (kind)
        {
            case EncryptionKind.Xor:
                return new XorEncryption(metadata);
            case EncryptionKind.Aes:
                return new AesEncryption(metadata);
            default:
                throw new NotImplementedException();
        }
    }
}