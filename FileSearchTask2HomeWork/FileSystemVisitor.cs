using System;
using System.Collections.Generic;
using System.IO;

namespace FileSystemView
{
    public class FileSystemVisitor
    {
        private readonly DirectoryInfo _rootDirectory;
        private readonly FilterDelegate _filter;

        public delegate bool FilterDelegate(FileSystemInfo item);
        public delegate void ItemFoundDelegate(object sender, ItemFoundEventArgs e);
        public delegate void SearchDelegate(object sender, EventArgs e);

        public event ItemFoundDelegate FileFound;
        public event ItemFoundDelegate DirectoryFound;
        public event ItemFoundDelegate FilteredFileFound;
        public event ItemFoundDelegate FilteredDirectoryFound;
        public event SearchDelegate StartSearch;
        public event SearchDelegate EndSearch;

        public FileSystemVisitor(string rootDirectoryPath) : this(rootDirectoryPath, null)
        {
        }

        public FileSystemVisitor(string rootDirectoryPath, FilterDelegate filter)
        {
            _rootDirectory = new DirectoryInfo(rootDirectoryPath);
            _filter = filter ?? DefaultFilter;
        }

        private static bool DefaultFilter(FileSystemInfo item)
        {
            return true;
        }

        public IEnumerable<FileSystemInfo> Traverse()
        {
            StartSearch?.Invoke(this, EventArgs.Empty);

            Stack<DirectoryInfo> directories = new Stack<DirectoryInfo>();
            directories.Push(_rootDirectory);

            bool abortSearch = false;

            while (directories.Count > 0 && !abortSearch)
            {
                DirectoryInfo currentDirectory = directories.Pop();

                FileSystemInfo[] items;
                try
                {
                    items = currentDirectory.EnumerateFileSystemInfos().ToArray();
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }

                foreach (FileSystemInfo item in items)
                {
                    if (abortSearch)
                    {
                        break;
                    }

                    if (item is DirectoryInfo itemDirectory)
                    {
                        directories.Push(itemDirectory);
                    }

                    ItemFoundEventArgs itemArgs = new ItemFoundEventArgs(item);
                    if (item is FileInfo)
                    {
                        FileFound?.Invoke(this, itemArgs);
                    }
                    else if (item is DirectoryInfo)
                    {
                        DirectoryFound?.Invoke(this, itemArgs);
                    }

                    if (itemArgs.AbortSearch)
                    {
                        abortSearch = true;
                        break;
                    }

                    if (itemArgs.ExcludeItem)
                    {
                        continue;
                    }

                    bool itemPassesFilter = _filter(item);

                    if (itemPassesFilter)
                    {
                        if (item is FileInfo)
                        {
                            FilteredFileFound?.Invoke(this, itemArgs);
                        }
                        else if (item is DirectoryInfo)
                        {
                            FilteredDirectoryFound?.Invoke(this, itemArgs);
                        }

                        if (!itemArgs.ExcludeItem)
                        {
                            yield return item;
                        }
                    }
                }
            }

            EndSearch?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ItemFoundEventArgs : EventArgs
    {
        public FileSystemInfo Item { get; }
        public bool ExcludeItem { get; set; }
        public bool AbortSearch { get; set; }

        public ItemFoundEventArgs(FileSystemInfo item)
        {
            Item = item;
            ExcludeItem = false;
            AbortSearch = false;
        }
    }
}