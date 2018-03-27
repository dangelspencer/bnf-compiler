using System;
using System.IO;

namespace BnfCompiler
{

    public enum ArgumentType
    {
        File,
        Scanner,
        Debug,
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
            var debug = false;
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
                else if (argType == ArgumentType.Debug)
                {
                    debug = true;
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
                var scanner = new Scanner(file, debug);
                Console.WriteLine("\n\n\nToken List:");
                while (scanner.Stack.Count != 0)
                {
                    var token = scanner.Stack.Pop();
                    Console.WriteLine($"Token: '{token.Value}' ({Enum.GetName(typeof(Type), token.Type)})");
                    Console.WriteLine($"Line {token.LineIndex + 1}, Column {token.CharIndex + 1}");
                    Console.WriteLine(scanner.FileLines[token.LineIndex]);
                    Console.WriteLine($"{GetFillerString(token.CharIndex, " ")}{GetFillerString(token.Value.Length, "^")}\n");
                }
            }
            else
            {
                var parser = new Parser(file, debug);
                var result = parser.Parse();

                if (debug)
                {
                    Console.WriteLine("\n\n\nParse Messages:");
                    foreach (var line in result.Messages)
                    {
                        Console.WriteLine(line);
                    }
                }
                if (debug) 
                {
                    Console.WriteLine("\n\n\nSymbol Table Contents:");
                    parser._table.PrintSymbols();
                }

                var parseStatus = result.Success ? "Success" : "Failed";
                Console.WriteLine($"\n\n\n\nParse Result: {parseStatus}");

                if(!result.Success) 
                {
                    foreach (var line in result.ErrorMessages)
                    {
                        Console.WriteLine(line);
                    }
                }            
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

            if (argument == "-d")
            {
                return ArgumentType.Debug;
            }

            return ArgumentType.Other;
        }

        static string GetFillerString(int length, string filler)
        {
            var str = "";
            for (int i = 0; i < length; i++)
            {
                str += filler;
            }
            return str;
        }
    }
}

/* TODO
    1. Debug flag
    2. Update expression type checking to account for casting 
    3. Update index checking to make sure that the index is within the bounds of the array
    4. Add a re-sync point
*/