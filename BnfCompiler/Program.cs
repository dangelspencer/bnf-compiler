using System;
using System.IO;

namespace BnfCompiler
{
    class Program
    {
        static int Main(string[] args)
        {
            // verify input arguments
            if (args.Length == 0)
            {
                System.Console.WriteLine("Error: No filename given. \nPlease use 'dotnet run <file_name>'");
                return 1;
            }
            if (args.Length > 1) 
            {
                System.Console.WriteLine("Error: Too many arguments.\nPlease use 'dotnet run <file_name>'");
                return 1;
            }

            // verify file exists
            var file = args[0];
            if (!File.Exists(file))
            {
                System.Console.WriteLine("Error: Unable to read file.\nPlease give the full name of the file to be processed");
                return 1;
            }

            System.Console.WriteLine("Processing file: " + file.Substring(file.LastIndexOf('/') + 1));

            var scanner = new Scanner(file);
            var result = scanner.Scan();
            while (result != null) 
            {
                Console.WriteLine(result);
                result = scanner.Scan();
            }

            return 0;
        }
    }
}
