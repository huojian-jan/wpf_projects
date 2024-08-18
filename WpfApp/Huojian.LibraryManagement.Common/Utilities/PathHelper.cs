using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public static class PathHelper
    {
        public static bool IsSamePath(string path1, string path2)
        {
            return NormalizePath(path1) == NormalizePath(path2);
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        public static string NormalizePath(Uri uri)
        {
            return Path.GetFullPath(uri.LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToUpperInvariant();
        }

        /// <summary>
        /// Returns a relative path string from a full path based on a base path
        /// provided.
        /// </summary>
        /// <param name="fullPath">The path to convert. Can be either a file or a directory</param>
        /// <param name="basePath">The base path on which relative processing is based. Should be a directory.</param>
        /// <returns>
        /// String of the relative path.
        ///
        /// Examples of returned values:
        ///  test.txt, ..\test.txt, ..\..\..\test.txt, ., .., subdir\test.txt
        /// </returns>
        public static string GetRelativePath(string fullPath, string basePath)
        {
            // Require trailing backslash for path
            if (!basePath.EndsWith("\\"))
                basePath += "\\";

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);


            // Uri's use forward slashes so convert back to backward slashes
            // new Uri 会将字符串中的特殊字符如：% +  等进行转移，此处之际返回toString()可能会拿到被转义的结果，需要解码一下
            return System.Web.HttpUtility.UrlDecode(relativeUri.ToString()).Replace("/", "\\");
        }

        public static void CopyFolder(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            var files = dir.GetFiles();
            foreach (var file in files)
                file.CopyTo(Path.Combine(destDirName, file.Name), false);

            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    CopyFolder(subdir.FullName, Path.Combine(destDirName, subdir.Name), copySubDirs);
                }
            }
        }

        public static void ClearFolder(string path)
        {
            var updateDir = new DirectoryInfo(path);
            foreach (FileInfo file in updateDir.EnumerateFiles())
                file.Delete();
            foreach (DirectoryInfo dir in updateDir.EnumerateDirectories())
                dir.Delete(true);
        }

        public static void ClearFolderFilesWithFilter(string path, List<string> extFilter)
        {
            var updateDir = new DirectoryInfo(path);

            foreach (FileInfo file in updateDir.EnumerateFiles())
            {
                if (extFilter?.Contains(file.Extension) ?? false)
                {
                    file.Delete();
                }
            }
        }
    }
}
