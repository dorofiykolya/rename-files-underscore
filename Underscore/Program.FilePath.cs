using System;
using System.IO;

namespace Underscore
{
    partial class Program
    {
        private class FilePath
        {
            public FileInfo SourceInfo;
            public string Source;
            public string Destination;
            public bool HasConflict;
            public bool IsValidName;

            public bool Move()
            {
                try
                {
                    SourceInfo.MoveTo(Destination);
                    return true;
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                }

                return false;
            }
        }
    }
}