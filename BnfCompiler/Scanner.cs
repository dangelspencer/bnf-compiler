using System;
using System.IO;

namespace BnfCompiler
{
    public class Scanner
    {
        readonly StreamReader _reader;

        public Scanner(string file)
        {
            FileStream fileStream = new FileStream(file, FileMode.Open);
            _reader = new StreamReader(fileStream);
        }

        public string Scan()
        {
            // TODO return null if end of file
            var token = (char)_reader.Read();

            //ignore whitespace
            while (!Char.IsWhiteSpace(token))
            {
                token = (char)_reader.Read();
            }

            if (Char.IsLetter(token))
            {
                //identifier
                var str = "";
                do
                {
                    str += token.ToString();
                    token = (char)_reader.Read();
                }
                while (Char.IsLetterOrDigit(token) || token == '_');
                return str;
            }
            if (Char.IsDigit(token))
            {
                //number
                var str = token.ToString();
                do
                {
                    token = (char)_reader.Read();
                } while (Char.IsDigit(token));

                return str;
            }
            if (Char.IsSymbol(token))
            {
                //symbol, char, string, or negative number

            }

            return null;
        }
    }
}
