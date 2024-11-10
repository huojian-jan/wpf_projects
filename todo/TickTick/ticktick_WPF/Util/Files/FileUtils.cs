// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Files.FileUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using ticktick_WPF.Resource;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util.Files
{
  public class FileUtils
  {
    public static bool FileEmptyOrNotExists(string path)
    {
      if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        return true;
      return File.Exists(path) && new FileInfo(path).Length == 0L;
    }

    public static DateTime FileModifyTime(string path)
    {
      return !FileUtils.FileExists(path) ? new DateTime() : new FileInfo(path).LastWriteTime;
    }

    public static bool FileExists(string path) => File.Exists(path);

    public static bool FileInFolder(string path, string folder)
    {
      return !string.IsNullOrEmpty(path) && !string.IsNullOrWhiteSpace(path) && !string.IsNullOrEmpty(folder) && !string.IsNullOrWhiteSpace(folder) && path.StartsWith(folder);
    }

    public static bool FolderExists(string path) => Directory.Exists(path);

    public static double FileSize(string path)
    {
      return !File.Exists(path) ? 0.0 : (double) new FileInfo(path).Length / 1024.0 / 1024.0;
    }

    public static bool FileOverSize(string path)
    {
      long userLimit = Utils.GetUserLimit(Constants.LimitKind.AttachmentSize);
      return File.Exists(path) && new FileInfo(path).Length > userLimit;
    }

    public static void CollectFileSize(string path)
    {
      UserActCollectUtils.AddClickEvent("task_detail", "upload_attachment_size", FileUtils.FileSize(path).ToString("F2"));
    }

    public static string TrimFileName(string fileName, int maxLength)
    {
      if (fileName.Length <= maxLength)
        return fileName;
      string withoutExtension = Path.GetFileNameWithoutExtension(fileName);
      if (withoutExtension.Length == fileName.Length)
        return fileName.Substring(0, maxLength);
      string str = fileName.Substring(withoutExtension.Length);
      return fileName.Substring(0, maxLength - str.Length) + str;
    }

    public static void ResetDbVersionFile()
    {
      string version = Utils.GetVersion();
      List<string> list = ((IEnumerable<string>) Directory.GetFiles(AppPaths.DataDir, "dbversion*.txt")).ToList<string>();
      if (list.Count == 0)
      {
        try
        {
          File.WriteAllText(AppPaths.DataDir + "dbversion" + version + ".txt", string.Empty);
        }
        catch
        {
        }
      }
      else
      {
        string sourceFileName = list[0];
        try
        {
          File.Move(sourceFileName, AppPaths.DataDir + "dbversion" + version + ".txt");
        }
        catch
        {
        }
      }
    }

    public static string ToValidFileName(string filename)
    {
      if (string.IsNullOrEmpty(filename))
        return filename;
      string directoryName = Path.GetDirectoryName(filename);
      filename = filename.Substring(directoryName != null ? directoryName.Length : 0);
      Regex regex = new Regex("[\\:\\*\\?\\|]+");
      return directoryName + regex.Replace(filename, "_");
    }

    public static List<string> GetPastFiles()
    {
      List<string> pastFiles = new List<string>();
      foreach (string fileDrop in Clipboard.GetFileDropList())
        pastFiles.Add(fileDrop);
      return pastFiles;
    }

    public static string SavePasteFile(string file, string name)
    {
      name = FileUtils.TrimFileName(name, 220 - AppPaths.ImageDir.Length);
      string str = AppPaths.ImageDir + name;
      if (File.Exists(str))
        str = AppPaths.ImageDir + Path.GetFileNameWithoutExtension(name) + Utils.GetGuid() + Path.GetExtension(name);
      if (File.Exists(file))
        File.Copy(file, str, true);
      return str;
    }
  }
}
