using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jenkins.ConsoleGUI
{
    public static class CommandLineHelper
    {
        public const string JobNameArgument = "-n";
        public const string OpenFolderArgument = "-o";
        public const string ZipBuildArgument = "-z";
        public const string UnzipBuildArgument = "-nz";
        public const string GetLatestBuildArgument = "-l";
        public const string InteractiveModeArgument = "-i";
        public const string DownloadsFolderArgument = "-d";

        public static List<string> ValueArguments = new()
        {
            JobNameArgument,
            DownloadsFolderArgument
        };

        public static List<string> NoValueArguments = new()
        {
            OpenFolderArgument,
            ZipBuildArgument,
            UnzipBuildArgument,
            GetLatestBuildArgument,
            InteractiveModeArgument
        };

        public static void PrintHelpMessage()
        {
            Console.WriteLine("Welcome to Jenkins console helper by Elias13");

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("To work with jenkins client provide following required arguments: \n");
            Console.WriteLine($"\t {JobNameArgument}  [string] (Name of jenkins job to search) \n\n");

            Console.WriteLine($"Also you can provide optional arguments: \n");
            Console.WriteLine($"\t {DownloadsFolderArgument}  [string] (Path to folder where downloaded build will have been located) \n");
            Console.WriteLine($"\t {GetLatestBuildArgument}  (If present latest job build will have been downloaded) \n");
            Console.WriteLine($"\t {ZipBuildArgument}  (If present downloaded build will be archived in .zip) (If -z/-nz weren't specified then .zip will have been extracted \n");
            Console.WriteLine($"\t {UnzipBuildArgument} (If present downloaded build will have been extracted) \n");
            Console.WriteLine($"\t {OpenFolderArgument}  (If present downloads folder will have been opened when downloading ends) \n");
            Console.WriteLine($"\t {InteractiveModeArgument}  (If present application will run in interactive mode) \n");
        }

        public static IDictionary<string, string?> ParseCommandLineArguments(string[] args)
        {
            var argumentsDictionary = new Dictionary<string, string?>();

            foreach (var argumentKey in ValueArguments)
            {
                var argumentValue = GetCommandLineArgumentValueByKey(args, argumentKey);
                if (!string.IsNullOrEmpty(argumentValue))
                {
                    argumentsDictionary.Add(argumentKey, argumentValue);
                }
            }

            foreach (var argumentKey in NoValueArguments)
            {
                var isArgumentPresent = CheckIfArgumentPresent(args, argumentKey);
                if (isArgumentPresent)
                {
                    argumentsDictionary.Add(argumentKey, null);
                }
            }

            return argumentsDictionary;
        }

        private static string? GetCommandLineArgumentValueByKey(string[] args, string argumentKey)
        {
            string? argumentValue = null;

            var argumentKeyIndex = Array.IndexOf(args, argumentKey);
            if (argumentKeyIndex != -1)
            {
                var argumentValueIndex = argumentKeyIndex + 1;
                if (argumentValueIndex < args.Length)
                {
                    argumentValue = args[argumentValueIndex];
                }
            }

            return argumentValue;
        }

        private static bool CheckIfArgumentPresent(string[] args, string argumentKey) => args.Any(arg => arg.Trim().ToLower() == argumentKey.Trim().ToLower());
    }
}
