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

            // read file
            System.Console.WriteLine("Processing file: " + args[0]);
            var file = args[0];

            string contents = "";
            try
            {
                //read contents of file
                FileStream fileStream = new FileStream(file, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    contents = reader.ReadToEnd();
                }
            } 
            catch (System.IO.FileNotFoundException) 
            {
                // catch error where file doesn't exist
                System.Console.WriteLine("Error: Could not read file. Please give the absolute path of the file.");
                return 1;
            }

            //test to check file contents
            for (int i = 0; i < contents.Length; i++)
            {
                System.Console.WriteLine(i + ": " + contents.Substring(i, 1));
            }

            return 0;
        }
    }
}
