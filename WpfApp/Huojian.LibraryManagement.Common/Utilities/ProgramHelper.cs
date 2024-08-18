using System.Runtime.CompilerServices;

namespace ShadowBot.Common.Utilities
{
    public class ProgramHelper
    {
        public static IEnumerable<ProgramInfo> GetInstalledProgram(Func<string, bool> filter = null)
        {
            var programDataPrograms = Path.Combine(Environment.GetFolderPath(
              Environment.SpecialFolder.CommonApplicationData),
              @"Microsoft\Windows\Start Menu\Programs");
            foreach (var item in GotFilesRec(programDataPrograms, new WshShell(), filter))
                yield return item;

            var appDataPrograms = Path.Combine(Environment.GetFolderPath(
               Environment.SpecialFolder.ApplicationData),
               @"Microsoft\Windows\Start Menu\Programs");
            foreach (var item in GotFilesRec(appDataPrograms, new WshShell(), filter))
                yield return item;
        }

        public static IEnumerable<ProgramInfo> GotFilesRec(string path, WshShell shell, Func<string, bool> filter = null)
        {
            var root = new DirectoryInfo(path);
            var files = root.GetFiles();

            foreach (var item in files)
            {
                if (item.Extension.ToLower() != ".lnk")  //过滤掉非lnk文件
                    continue;

                ProgramInfo target = default;
                try
                {
                    var shortcut = (IWshShortcut)shell.CreateShortcut(item.FullName);
                    target.TargetPath = shortcut.TargetPath;
                    target.IconLocation = shortcut.IconLocation;
                    target.Name = item.Name;

                    if (filter != null)
                        if (!filter.Invoke(target.TargetPath))
                            continue;
                }
                catch
                {
                    continue;
                }
                yield return target;
            }

            var dirs = root.GetDirectories();
            foreach (var dir in dirs)
                foreach (var item in GotFilesRec(dir.FullName, shell, filter))
                    yield return item;
        }

        public static async IAsyncEnumerable<ProgramInfo> GetInstalledProgramAsync(Func<string, Task<bool>> filter = null,
                                                                                   [EnumeratorCancellation] CancellationToken token = default)
        {
            var programDataPrograms = Path.Combine(Environment.GetFolderPath(
              Environment.SpecialFolder.CommonApplicationData),
              @"Microsoft\Windows\Start Menu\Programs");
            await foreach (var item in GotFilesRecAsync(programDataPrograms, new WshShell(), filter,token))
                yield return item;

            var appDataPrograms = Path.Combine(Environment.GetFolderPath(
               Environment.SpecialFolder.ApplicationData),
               @"Microsoft\Windows\Start Menu\Programs");
            await foreach (var item in GotFilesRecAsync(appDataPrograms, new WshShell(), filter,token))
                yield return item;
        }

        public static async IAsyncEnumerable<ProgramInfo> GotFilesRecAsync(string path,
                                                                           WshShell shell,
                                                                           Func<string, Task<bool>> filter = null,
                                                                           [EnumeratorCancellation] CancellationToken token = default)
        {
            var root = new DirectoryInfo(path);
            var files = root.GetFiles();

            foreach (var item in files)
            {
                if (token != CancellationToken.None && token.IsCancellationRequested)
                    token.ThrowIfCancellationRequested();

                if (item.Extension.ToLower() != ".lnk")  //过滤掉非lnk文件
                    continue;

                ProgramInfo target = default;
                try
                {
                    var shortcut = (IWshShortcut)shell.CreateShortcut(item.FullName);
                    var targetFilePath = shortcut.TargetPath;

                    // https://stackoverflow.com/questions/7120583/accessing-target-path-from-a-shortcut-file-on-a-64-bit-system-using-32-bit-appli
                    // 32 位下获取文件路径拿到 Program Files (x86) 下的路径，会有不准确的情况 
                    // 64 位下获取文件路径拿到 Program Files 下的路径，会有不准确的情况
                    // 在未找到文件的情况下需要尝试去别的路径下进行检查
                    if (targetFilePath != null && !System.IO.File.Exists(targetFilePath))
                    {
                        if (targetFilePath.Contains("Program Files (x86)"))
                            targetFilePath = targetFilePath.Replace("Program Files (x86)", "Program Files");
                        else if (targetFilePath.Contains("Program Files"))
                            targetFilePath = targetFilePath.Replace("Program Files", "Program Files (x86)");

                        if (!System.IO.File.Exists(targetFilePath))
                            continue;
                    }
                    target.TargetPath = targetFilePath;
                    target.IconLocation = shortcut.IconLocation;
                    target.Name = item.Name;

                    if (filter != null)
                        if (!await filter.Invoke(target.TargetPath))
                            continue;
                }
                catch
                {
                    continue;
                }
                yield return target;
            }

            var dirs = root.GetDirectories();
            foreach (var dir in dirs)
            {
                await foreach (var item in GotFilesRecAsync(dir.FullName, shell, filter,token))
                    yield return item;
            }
               
        }

    }

    public struct ProgramInfo
    {
        public string TargetPath { get; set; }

        public string Name { get; set; }

        public string IconLocation { get; set; }
    }

}
