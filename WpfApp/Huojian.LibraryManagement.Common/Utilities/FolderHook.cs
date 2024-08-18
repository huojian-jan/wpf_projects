using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class FolderHook : IDisposable
    {
        private FileSystemWatcher _fileSystemWatcher;
        public FileSystemWatcher FileSystemWatcher => _fileSystemWatcher;

        private WatcherChangeTypes _watcherChangeTypes;

        /// <summary>
        /// filefolder hook
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="filter">
        ///     https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher.filter?redirectedfrom=MSDN&view=netframework-4.8#System_IO_FileSystemWatcher_Filter
        ///     *.*                 All files (default). An empty string ("") also watches all files.
        ///     *.txt               All files with a "txt" extension.
        ///     *recipe.doc         All files ending in "recipe" with a "doc" extension.
        ///     win*.xml            All files beginning with "win" with an "xml" extension.
        ///     Sales*200?.xls      Match：Sales July 2001.xls、Sales Aug 2002.xls，   Not Match: Sales Nov 1999.xls
        ///     MyReport.Doc        Watches only MyReport.doc
        /// </param>
        public FolderHook(string folderPath, string filter = "*.*", bool includeSubdirectories = true, WatcherChangeTypes watcherChangeTypes = WatcherChangeTypes.All)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);


            _fileSystemWatcher = new FileSystemWatcher(folderPath, filter);

            _watcherChangeTypes = watcherChangeTypes;

            // Watch for changes in LastAccess and LastWrite times, and
            // the renaming of files or directories.
            //_fileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess
            //                     | NotifyFilters.LastWrite
            //                     | NotifyFilters.FileName
            //                     | NotifyFilters.DirectoryName;

            _fileSystemWatcher.IncludeSubdirectories = includeSubdirectories;

            StartMonitor();
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            FolderChanged?.Invoke(this, new FolderChangedArgs(e));
        }
        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            FolderChanged?.Invoke(this, new FolderChangedArgs(e));
        }
        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            FolderChanged?.Invoke(this, new FolderChangedArgs(e));
        }
        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            FolderChanged?.Invoke(this, new FolderChangedArgs(e));
        }

        public event EventHandler<FolderChangedArgs> FolderChanged;

        public void Dispose()
        {
            StopMonitor();

            _fileSystemWatcher.Dispose();
            _fileSystemWatcher = null;

            FolderChanged = null;
        }

        public void StartMonitor()
        {
            if (_watcherChangeTypes.HasFlag(WatcherChangeTypes.Created))
                _fileSystemWatcher.Created += FileSystemWatcher_Created;

            if (_watcherChangeTypes.HasFlag(WatcherChangeTypes.Renamed))
                _fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;

            if (_watcherChangeTypes.HasFlag(WatcherChangeTypes.Deleted))
                _fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;

            if (_watcherChangeTypes.HasFlag(WatcherChangeTypes.Changed))
                _fileSystemWatcher.Changed += FileSystemWatcher_Changed;

            // Begin watching.
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        public void StopMonitor()
        {
            if (_watcherChangeTypes.HasFlag(WatcherChangeTypes.Created))
                _fileSystemWatcher.Created -= FileSystemWatcher_Created;

            if (_watcherChangeTypes.HasFlag(WatcherChangeTypes.Renamed))
                _fileSystemWatcher.Renamed -= FileSystemWatcher_Renamed;

            if (_watcherChangeTypes.HasFlag(WatcherChangeTypes.Deleted))
                _fileSystemWatcher.Deleted -= FileSystemWatcher_Deleted;

            if (_watcherChangeTypes.HasFlag(WatcherChangeTypes.Changed))
                _fileSystemWatcher.Changed -= FileSystemWatcher_Changed;

            // End watching.
            _fileSystemWatcher.EnableRaisingEvents = false;
        }
    }

    public class FolderChangedArgs : EventArgs
    {
        public WatcherChangeTypes ChangeType { get; set; }

        public string FullPath { get; set; }

        public string Name { get; set; }

        public string OldFullPath { get; set; }

        public string OldName { get; set; }

        // Created、Deleted、Changed
        public FileSystemEventArgs FileSystemEventArgs { get; set; }
        // Renamed
        public RenamedEventArgs RenamedEventArgs { get; set; }

        public FolderChangedArgs(FileSystemEventArgs fileSystemEventArgs)
        {
            this.FileSystemEventArgs = fileSystemEventArgs;
            ChangeType = FileSystemEventArgs.ChangeType;
            FullPath = FileSystemEventArgs.FullPath;
            Name = FileSystemEventArgs.Name;
        }

        public FolderChangedArgs(RenamedEventArgs renamedEventArgs) : this(renamedEventArgs as FileSystemEventArgs)
        {
            this.RenamedEventArgs = renamedEventArgs;
            OldFullPath = RenamedEventArgs?.OldFullPath;
            OldName = RenamedEventArgs?.OldName;
        }
    }
}
