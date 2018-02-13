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
            if (_reader.EndOfStream) {
                return null;
            }
            var token = (char)_reader.Read();

            //ignore whitespace
            while (Char.IsWhiteSpace(token) || token == '\n')
            {
                if (_reader.EndOfStream) 
                {
                    return null;
                }

                token = (char)_reader.Read();
            }

            if (Char.IsSymbol(token)) {
                return token.ToString();
            }

            if (Char.IsLetter(token))
            {
                //identifier
                var str = token.ToString();
                while (Char.IsLetterOrDigit((char)_reader.Peek()) || (char)_reader.Peek() == '_')
                {
                    token = (char)_reader.Read();
                    str += token.ToString();
                }
                return str.ToUpper();
            }
            if (Char.IsDigit(token))
            {
                //number
                var str = token.ToString();
                var hasDecimal = false;
                while(Char.IsDigit((char)_reader.Peek()) || (char)_reader.Peek() == '.' || (char)_reader.Peek() == '_') 
                {
                    token = (char)_reader.Read();
                    if (!hasDecimal && token == '.')
                    {
                        hasDecimal = true;
                    }
                    else if (token == '.')
                    {
                        return str; //TODO throw error about unexpected '.'
                    }

                    str += token.ToString();
                }

                return str.ToUpper();
            }
            if (token == '/' && (char)_reader.Peek() == '/')
            {
                // single line comment
                while (token != '\n')
                {
                    token = (char)_reader.Read();
                }
                return Scan(); // get the next token after the comment
            }
            if (token == '/' && (char)_reader.Peek() == '*')
            {
                // block comment
                token = (char)_reader.Read(); //token = *
                var level = 1;
                do
                {
                    token = (char)_reader.Read();
                    if (token == '*' && (char)_reader.Peek() == '/')
                    {
                        level--;
                        _reader.Read();
                        continue;
                    }
                    if (token == '/' && (char)_reader.Peek() == '*')
                    {
                        level++;
                    }
                }
                while (level > 0);
                return Scan();
            }
            if (token == '"')
            {
                // string
                var str = "";
                var valid = true;
                do
                {
                    token = (char)_reader.Read();
                    if (!Char.IsLetterOrDigit(token) || token != '_' || token != ';' || token != ':' || token != '.' || token != '\'' || token == '"')
                    {
                        valid = false;
                    }
                    else
                    {
                        str += token.ToString();
                    }
                }
                while (valid);
                return str.ToUpper();
            }
            if (token == '\'')
            {
                // char 
                if ((char)_reader.Peek() == '\'')
                {
                    _reader.Read();
                    return "";
                }
                token = (char)_reader.Read();
                if ((char)_reader.Peek() != '\'')
                {
                    throw new Exception("Multi-character chars are not allowed");
                }
                return token.ToString();
            }
            return token.ToString();
        }
    }
}
