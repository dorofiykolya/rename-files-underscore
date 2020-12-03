using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Underscore
{
    partial class Program
    {
        private static readonly ConsoleColor ForegroundColor = Console.ForegroundColor;
        private static readonly ConsoleColor BackgroundColor = Console.BackgroundColor;

        static readonly bool IsNameRegister = Environment.OSVersion.Platform == PlatformID.Unix ||
                                              Environment.OSVersion.Platform == PlatformID.MacOSX;

        static void PrintHelp(Arguments arguments)
        {
            Console.WriteLine("arguments:");
            Console.WriteLine(Utils.GetArgumentsDescription(typeof(ArgumentConstants), 2));
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("error: no arguments,   files|directories [-argument1 --argument2 ...]");
                Console.WriteLine("example: ");
                Console.WriteLine(@"  ""file1.jpg"" ""file2.png"" ""directory1"" ""directory2"" -verbose");

                PrintHelp(Utils.GetCommandLineArguments(args));
                Environment.ExitCode = 1;
                return;
            }

            var arguments = Utils.GetCommandLineArguments(args);
            if (arguments.Contains(ArgumentConstants.Help))
            {
                PrintHelp(arguments);
            }
            else
            {
                Console.WriteLine();

                Map map = new Map();

                foreach (var path in arguments.Paths)
                {
                    if (File.Exists(path))
                    {
                        ProcessFile(new FileInfo(path), map, arguments);
                    }
                    else if (Directory.Exists(path))
                    {
                        ProcessDirectory(new DirectoryInfo(path), map, arguments);
                    }
                }

                if (map.HasProblems)
                {
                    if (arguments.GetAsBool(ArgumentConstants.Status))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("status: fail");
                        Console.ForegroundColor = ForegroundColor;
                    }

                    if (!arguments.GetAsBool(ArgumentConstants.Status) || arguments.IsVerbose)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;

                        if (map.InvalidNames.Length != 0)
                        {
                            Console.WriteLine("INVALID NAMES (NO SUPPORTED CHARS)");
                            Console.WriteLine("-----------------------------------");
                            foreach (var filePath in map.InvalidNames)
                            {
                                Console.WriteLine($"{filePath.Source} => {filePath.Destination}");
                            }

                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine();
                        }

                        if (map.Conflicts.Length != 0)
                        {
                            Console.WriteLine("CONFLICTS (file exists)");
                            Console.WriteLine("-----------------------------------");
                            foreach (var filePath in map.Conflicts)
                            {
                                Console.WriteLine($"{filePath.Source} => {filePath.Destination}");
                            }

                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine();
                        }

                        if (map.DestinationConflicts.Length != 0)
                        {
                            Console.WriteLine("CONFLICTS DESTINATIONS (file exists)");
                            Console.WriteLine("-----------------------------------");
                            foreach (var conflict in map.DestinationConflicts)
                            {
                                Console.WriteLine("[");
                                var maxLen = conflict.Conflicts.Max(s => s.Source.Length);
                                var lines = conflict.Conflicts.Select(s => s.Source.PadRight(maxLen) + "  |  ")
                                    .ToArray();
                                lines[0] += conflict.Destination;
                                foreach (var line in lines)
                                {
                                    Console.WriteLine($"\t{line}");
                                }

                                Console.WriteLine("]");
                            }

                            Console.WriteLine("-----------------------------------");
                            Console.WriteLine();
                        }

                        Console.ForegroundColor = ForegroundColor;
                    }
                }
                else
                {
                    if (arguments.GetAsBool(ArgumentConstants.Status))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("status: ok");
                        Console.ForegroundColor = ForegroundColor;
                        if (arguments.IsVerbose)
                        {
                            foreach (var file in map.Files)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"move: {file.Source} => {file.Destination}");
                                Console.ForegroundColor = ForegroundColor;
                            }
                        }
                    }
                    else
                    {
                        foreach (var file in map.Files)
                        {
                            var counts = 5;
                            while (!file.Move() && --counts >= 0)
                            {
                                Thread.Sleep(10);
                            }

                            if (counts <= 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"FAIL to move file: {file.Source} => {file.Destination}");
                            }
                            else
                            {
                                if (arguments.IsVerbose)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"move: {file.Source} => {file.Destination}");
                                }
                            }
                        }
                    }
                }
            }

            if (arguments.GetAsBool(ArgumentConstants.WaitPressEnter))
            {
                Console.WriteLine();
                Console.WriteLine("Press ENTER to exit");

                while (true)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        return;
                    }
                }
            }
        }

        private static void ProcessDirectory(DirectoryInfo info, Map map, Arguments arguments)
        {
            var dirs = info.GetDirectories();
            var files = info.GetFiles();

            foreach (var fileInfo in files)
            {
                ProcessFile(fileInfo, map, arguments);
            }

            foreach (var dir in dirs)
            {
                ProcessDirectory(dir, map, arguments);
            }
        }

        private static void ProcessFile(FileInfo info, Map map, Arguments arguments)
        {
            var name = Path.GetFileNameWithoutExtension(info.Name);
            var newName = ToUnderscore(name);
            if (newName != name || !IsValidName(newName))
            {
                if (arguments.GetAsBool(ArgumentConstants.Force))
                {
                    newName = ForceReplaceChars(newName, "_icr_");
                }

                var destination = new FileInfo(Path.Combine(info.DirectoryName, newName + info.Extension));
                var filePath = new FilePath
                {
                    SourceInfo = info,
                    Source = info.FullName,
                    Destination = destination.FullName,
                    IsValidName = IsValidName(newName)
                };
                if (IsNameRegister)
                {
                    filePath.HasConflict =
                        destination.Exists && Path.GetFileNameWithoutExtension(destination.Name) != name;
                }
                else
                {
                    if (destination.Exists)
                    {
                        if (Path.GetFileNameWithoutExtension(destination.Name) != name.ToLowerInvariant())
                        {
                            filePath.HasConflict = true;
                        }
                    }
                }

                map.Add(filePath);
            }
        }

        public static string ToUnderscore(string name)
        {
            string fileName = name;
            var newFileName = Regex.Replace(fileName, "(?<=[a-z0-9])[A-Z]", m => "_" + m.Value);
            newFileName = newFileName.Replace("-", "_");
            newFileName = newFileName.ToLowerInvariant();
            newFileName = newFileName.Replace(' ', '_');
            return newFileName;
        }

        public static string ForceReplaceChars(string name, string replace)
        {
            var builder = new StringBuilder(name.Length);
            foreach (var c in name)
            {
                if (IsValidChar(c))
                {
                    builder.Append(c);
                }
                else
                {
                    if (ReplaceMap.ContainsKey(c))
                    {
                        builder.Append(ReplaceMap[c]);
                    }
                    else
                    {
                        builder.Append(replace);
                    }
                }
            }

            return builder.ToString();
        }

        public static bool IsValidName(string name)
        {
            foreach (var c in name)
            {
                if (!IsValidChar(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidChar(char c)
        {
            if ((c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') ||
                (c == '_') ||
                (c == '.')
            )
            {
                return true;
            }

            return false;
        }
    }
}