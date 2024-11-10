// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.IOUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class IOUtils
  {
    public static string GetFileContentInAssemblyFold(string fileName)
    {
      string contentInAssemblyFold = string.Empty;
      string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      if (directoryName != null)
        contentInAssemblyFold = IOUtils.GetFileContent(Path.Combine(directoryName, fileName));
      return contentInAssemblyFold;
    }

    private static string GetFileContent(string path)
    {
      string fileContent = string.Empty;
      if (File.Exists(path))
      {
        StreamReader streamReader = new StreamReader(path);
        fileContent = streamReader.ReadToEnd();
        streamReader.Close();
      }
      return fileContent;
    }

    public static string GetFileContentInResourceFile(string filePath)
    {
      StreamResourceInfo resourceStream = Application.GetResourceStream(new Uri("pack://application:,,," + filePath));
      if (resourceStream == null)
        return string.Empty;
      using (StreamReader streamReader = new StreamReader(resourceStream.Stream, Encoding.UTF8))
        return streamReader.ReadToEnd();
    }

    public static async Task<bool> CheckResourceExist(
      string dir,
      string fileName,
      string remotePath)
    {
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
      return await Utils.TryDownloadFile(remotePath, dir + fileName);
    }

    public static async Task<bool> DownloadFile(string dir, string fileName, string remotePath)
    {
      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);
      return await Utils.TryDownloadFile(remotePath, dir + fileName, true);
    }

    public static void DeleteFile(string path)
    {
      if (!File.Exists(path))
        return;
      FileInfo fileInfo = new FileInfo(path);
      fileInfo.Attributes = FileAttributes.Normal;
      fileInfo.Delete();
    }

    public static string GetFileName(string path)
    {
      if (path == null)
        return (string) null;
      return ((IEnumerable<string>) ((IEnumerable<string>) path.Split('\\')).Last<string>().Split('/')).Last<string>();
    }

    public static byte[] ReadBytes(Func<byte[], int, int, int> readFunc, int bufLen)
    {
      byte[] buffer = new byte[bufLen];
      using (MemoryStream memoryStream = new MemoryStream())
      {
        while (true)
        {
          int count = readFunc(buffer, 0, buffer.Length);
          if (count != 0)
            memoryStream.Write(buffer, 0, count);
          else
            break;
        }
        return memoryStream.ToArray();
      }
    }

    public static string ReadString(Func<byte[], int, int, int> readFunc, Encoding encoding = null)
    {
      byte[] bytes = IOUtils.ReadBytes(readFunc, 1024);
      encoding = encoding ?? Encoding.Unicode;
      return encoding.GetString(bytes, 0, bytes.Length);
    }
  }
}
