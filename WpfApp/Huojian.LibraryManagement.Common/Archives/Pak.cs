using System.Text;

namespace Huojian.LibraryManagement.Common.Archives;
public class Pak
{
    /*
        [SEG]:
        [Length][Content]
        [  4B  ][  *B   ]

        [File]:
        [MagicNum][Flag][Encryption][Index-names][Index-offsets][Content]
        [   4B   ][ 1B ][   SEG    ][    SEG    ][     SEG     ][  SEG  ]
    */

    private static readonly int _magicNum = 65748392;
    private static readonly Encoding _encoding = new UTF8Encoding(false);

    private readonly string _filename;
    private readonly IEncryption _encryption;
    private readonly FileStream _fileStream;
    private readonly Dictionary<string, PFile> _fileDict;

    public Pak(string filename)
    {
        _filename = filename;
        try
        {
            int headLength = 0;
            _fileStream = File.OpenRead(filename);
            // valid magic num
            var count = ValidMagicNumber(_fileStream);
            headLength += count;
            // read kind 
            var kind = ReadEncryptionKind(_fileStream);
            headLength += 1;
            // read metadata
            count = ReadSegment(_fileStream, out byte[] metadata);
            headLength += count;
            // init IEncryption
            _encryption = EncryptionFactory.CreateEncryption(kind, metadata);
            // read index names
            count = ReadAESSegment(_fileStream, _encryption, out byte[] nameBytes);
            headLength += count;
            // read index offset
            count = ReadAESSegment(_fileStream, _encryption, out byte[] offsetBytes);
            headLength += count;
            // init file dict
            _fileDict = new Dictionary<string, PFile>();
            var names = _encoding.GetString(nameBytes).Split('|');
            var offsets = _encoding.GetString(offsetBytes).Split('|')
                .Select(m => int.Parse(m))
                .ToArray();
            for (int i = 0; i < names.Length; i++)
                _fileDict[names[i]] = new PFile(names[i], headLength + offsets[i]);
        }
        catch
        {
            if (_fileStream != null)
                _fileStream.Dispose();
            if (_encryption != null)
                _encryption.Dispose();
            throw;
        }
    }

    public string ReadText(string relativePath, Encoding encoding = null)
    {
        var bytes = ReadBytes(relativePath);
        if (encoding == null)
            encoding = _encoding;
        return encoding.GetString(bytes);
    }

    public byte[] ReadBytes(string relativePath)
    {
        var key = relativePath.ToUpper().Replace('/', '\\');
        if (!_fileDict.TryGetValue(key, out PFile pFile))
            throw new FileNotFoundException($"{String.Format(Strings.Pak_FileNotFoundAt, _filename)}{relativePath}");
        _fileStream.Seek(pFile.Offset, SeekOrigin.Begin);
        ReadAESSegment(_fileStream, _encryption, out byte[] bytes);
        return bytes;
    }

    private int ValidMagicNumber(FileStream fileStream)
    {
        // valid magic number
        var bytes = new byte[4];
        fileStream.Read(bytes, 0, bytes.Length);
        if (BitConverter.ToInt32(bytes, 0) != _magicNum)
            throw new IOException($"{Strings.Pak_InvalidPakFile}");
        return 4;
    }

    private EncryptionKind ReadEncryptionKind(FileStream fileStream)
    {
        var value = fileStream.ReadByte();
        if (Enum.TryParse(value.ToString(), out EncryptionKind kind))
            return kind;
        else
            throw new IOException($"{Strings.Pak_CouldNotParseThisPakFile}");
    }

    public void Dispose()
    {
        if (_fileStream != null)
            _fileStream.Dispose();
        if (_encryption != null)
            _encryption.Dispose();
    }

    #region unpack

    private static int ReadSegment(FileStream fileStream, out byte[] bytes)
    {
        bytes = new byte[4];
        fileStream.Read(bytes, 0, bytes.Length);
        var length = BitConverter.ToInt32(bytes, 0);
        bytes = new byte[length];
        fileStream.Read(bytes, 0, bytes.Length);
        return bytes.Length + 4;
    }

    private static int ReadAESSegment(FileStream fileStream, IEncryption decryptor, out byte[] bytes)
    {
        var count = ReadSegment(fileStream, out bytes);
        bytes = decryptor.Decrypt(bytes);
        return count;
    }

    #endregion

    #region pack

    public static void Pack(string folder, string targetFilePath, EncryptionKind kind)
    {
        var index = new List<PFile>();
        var contentFilePath = targetFilePath + ".tmp";
        using (var encryption = EncryptionFactory.CreateEncryption(kind))
        {
            using (var contentFileStream = new FileStream(contentFilePath, FileMode.OpenOrCreate))
            {
                var offset = 0;
                WriteFolder(folder, contentFileStream, encryption, ref index, ref offset);
                using (var fileStream = File.Create(targetFilePath))
                {
                    // write magic num
                    fileStream.Write(BitConverter.GetBytes(_magicNum), 0, 4);
                    // write flag
                    fileStream.WriteByte((byte)kind);
                    // write metadata
                    WriteSegment(fileStream, encryption.Metadata);
                    // write index-names
                    var indexNames = string.Join("|", index.Select(m => GetRelativePath(m.Path, folder)));
                    WriteAESSegment(fileStream, _encoding.GetBytes(indexNames), encryption);
                    // write index-offsets
                    var indexOffsets = string.Join("|", index.Select(m => m.Offset));
                    WriteAESSegment(fileStream, _encoding.GetBytes(indexOffsets), encryption);
                    contentFileStream.Seek(0, SeekOrigin.Begin);
                    contentFileStream.CopyTo(fileStream);
                }
            }
        }
        File.Delete(contentFilePath);
    }

    private static void WriteFolder(string path, FileStream fileStream, IEncryption encryptor,
        ref List<PFile> index, ref int offset)
    {
        foreach (var file in Directory.EnumerateFiles(path))
            index.Add(WriteFile(file, fileStream, encryptor, ref offset));

        foreach (var folder in Directory.EnumerateDirectories(path))
            WriteFolder(folder, fileStream, encryptor, ref index, ref offset);
    }

    private static PFile WriteFile(string path, FileStream fileStream, IEncryption encryptor,
        ref int offset)
    {
        var bytes = File.ReadAllBytes(path);
        var count = WriteAESSegment(fileStream, bytes, encryptor);
        var pfile = new PFile(path, offset);
        offset += count;
        return pfile;
    }

    private static int WriteSegment(FileStream fileStream, byte[] bytes)
    {
        var length = bytes.Length;
        fileStream.Write(BitConverter.GetBytes(length), 0, 4);
        fileStream.Write(bytes, 0, length);
        return length + 4;
    }

    private static int WriteAESSegment(FileStream fileStream, byte[] bytes, IEncryption encryptor)
    {
        var encrypted = encryptor.Encrypt(bytes);
        return WriteSegment(fileStream, encrypted);
    }

    #endregion

    private static string GetRelativePath(string filespec, string folder)
    {
        Uri pathUri = new Uri(filespec);
        // Folders must end in a slash
        if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            folder += Path.DirectorySeparatorChar;
        Uri folderUri = new Uri(folder);
        return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri)
            .ToString()
            .ToUpper()
            .Replace('/', '\\'));
    }


    class PFile
    {
        public PFile(string path, int offset)
        {
            Path = path;
            Offset = offset;
        }

        public string Path { get; }

        public int Offset { get; }
    }
}