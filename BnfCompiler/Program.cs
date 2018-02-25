using System;
using System.IO;

namespace BnfCompiler
{

    public enum ArgumentType
    {
        File,
        Scanner,
        Other
    }

    class Program
    {
        static int Main(string[] args)
        {
            // verify input arguments
            if (args.Length == 0)
            {
                System.Console.WriteLine("Error: No filename given. \nPlease use 'dotnet run --file <file_name>'");
                return 1;
            }
            // if (args.Length > 1) 
            // {
            //     System.Console.WriteLine("Error: Too many arguments.\nPlease use 'dotnet run <file_name>'");
            //     return 1;
            // }

            var file = "";
            var scannerOnly = false;
            for (var i = 0; i < args.Length; i++)
            {
                var argType = CheckArgumentType(args[i]);
                if (argType == ArgumentType.File)
                {
                    argType = CheckArgumentType(args[i + 1]);
                    if (argType != ArgumentType.Other)
                    {
                        throw new Exception("Invalid command line arguments");
                    }
                    else
                    {
                        file = args[i + 1];
                        i = i + 1;
                        continue;
                    }
                }
                else if (argType == ArgumentType.Scanner)
                {
                    scannerOnly = true;
                }
            }

            // verify file exists
            if (file == "")
            {
                throw new Exception("File not provided");
            }

            if (!File.Exists(file))
            {
                System.Console.WriteLine("Error: Unable to read file.\nPlease give the full name of the file to be processed");
                return 1;
            }

            System.Console.WriteLine("Processing file: " + file.Substring(file.LastIndexOf('/') + 1));

            if (scannerOnly)
            {
                Console.WriteLine("Token List:");
                var nullString = "(null)";
                var emptyString = "";
                var scanner = new Scanner(file);
                var result = scanner.Stack.Pop();
                while (result != null)
                {
                    var nullValue = result.Value == null ? nullString : emptyString;
                    Console.WriteLine($"({result.LineIndex}, {result.CharIndex}) {result.Value} {nullValue}");
                    result = scanner.Stack.Pop();
                }
            }
            else
            {
                var parser = new Parser(file);
                var result = parser.Parse();

                foreach (var line in result.Messages)
                {
                    Console.WriteLine(line);
                }

                var parseStatus = result.Success ? "Success" : "Failed";
                Console.WriteLine($"\nParse Result: {parseStatus}");
            }

            return 0;
        }

        static ArgumentType CheckArgumentType(string argument)
        {
            if (argument == "--file" || argument == "-f")
            {
                return ArgumentType.File;
            }

            if (argument == "--scanner" || argument == "-s")
            {
                return ArgumentType.Scanner;
            }

            return ArgumentType.Other;
        }
    }
}
