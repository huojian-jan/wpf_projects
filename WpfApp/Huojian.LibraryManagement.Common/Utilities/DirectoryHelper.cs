namespace ShadowBot.Common.Utilities
{
    public class DirectoryHelper
    {
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = false, bool @override = false)
        {
            var dir = new DirectoryInfo(sourceDir);

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, @override);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true, @override);
                }
            }
        }

        public static bool DeleteFolderWithRetry(string folder)
        {
            if (Directory.Exists(folder))
            {
                return Policy.Retry(() =>
                {
                    try
                    {
                        Directory.Delete(folder, true);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }, 3000, interval: 500);
            }
            return true;
        }

        public static long GetDirectorySize(string folderPath, SearchOption searchOption)
        {
            try
            {
                long size = 0;
                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                foreach (FileInfo file in directoryInfo.GetFiles("*", searchOption))
                {
                    size += file.Length;
                }

                return size;
            }
            catch { }
            return 0;
        }
    }
}
