using FileSystemView;
using System;

namespace FileSystemView
{
    class Program
    {
        static void Main(string[] args)
        {
            string rootDirectoryPath = @"C:\Users\Aliaksandr_Makhnach\source\repos\FileSystemHomeWork\";

            var fileSystemVisitor = new FileSystemVisitor(rootDirectoryPath);
            fileSystemVisitor.FileFound += OnFileFound;
            fileSystemVisitor.DirectoryFound += OnDirectoryFound;
            fileSystemVisitor.FilteredFileFound += OnFilteredFound;
            fileSystemVisitor.FilteredDirectoryFound += OnFilteredFound;
            fileSystemVisitor.StartSearch += (sender, e) => Console.WriteLine("Search start");
            fileSystemVisitor.EndSearch += (sender, e) => Console.WriteLine("Search end");

            foreach (var item in fileSystemVisitor.Traverse())
            {
                Console.WriteLine(item.FullName);
            }

            Console.ReadLine();
        }

        private static void OnFileFound(object sender, ItemFoundEventArgs e)
        {
            Console.WriteLine($"File found: {e.Item.FullName}");
        }

        private static void OnDirectoryFound(object sender, ItemFoundEventArgs e)
        {
            Console.WriteLine($"Directory found: {e.Item.FullName}");
        }

        private static void OnFilteredFound(object sender, ItemFoundEventArgs e)
        {
            Console.WriteLine($"Filtered item: {e.Item.FullName}");

            e.AbortSearch = true;
        }
    }
}