using System;
using System.Collections.Generic;
using System.IO;

namespace BnfCompiler
{
    public class Scanner
    {
        readonly StreamReader _reader;
        public Stack<Token> _stack;
        public bool EndOfFile;
        public List<string> FileLines = new List<string>();
        public int _lineIndex = 0;
        public int _charIndex = 0;
        public string _currentLine = "";

        public Scanner(string file)
        {
            FileStream fileStream = new FileStream(file, FileMode.Open);
            _reader = new StreamReader(fileStream);
            _stack = new Stack<Token>();

            var tokenList = new List<Token>();

            while (!_reader.EndOfStream)
            {
                var token = getNextToken();

                //ignore whitespace
                while (Char.IsWhiteSpace(token) || token == '\n')
                {
                    if (_reader.EndOfStream)
                    {
                        break;
                    }

                    token = getNextToken();
                }

                if (Char.IsSymbol(token))
                {
                    tokenList.Add(new Token(token.ToString(), Type.SPECIAL, _lineIndex, _charIndex));
                    continue;
                }

                if (Char.IsLetter(token))
                {
                    //identifier
                    var str = token.ToString();
                    while (Char.IsLetterOrDigit((char)_reader.Peek()) || (char)_reader.Peek() == '_')
                    {
                        token = getNextToken();
                        str += token.ToString();
                    }
                    tokenList.Add(new Token(str.ToUpper(), Type.IDENTIFIER, _lineIndex, _charIndex));
                    continue;
                }
                if (Char.IsDigit(token))
                {
                    //number
                    var str = token.ToString();
                    var hasDecimal = false;
                    while (Char.IsDigit((char)_reader.Peek()) || (char)_reader.Peek() == '.' || (char)_reader.Peek() == '_')
                    {
                        token = (char)_reader.Peek();
                        if (!hasDecimal && token == '.')
                        {
                            hasDecimal = true;
                            token = getNextToken();
                            str += token.ToString();
                        }
                        else if (token == '.')
                        {
                            tokenList.Add(new Token(str.ToUpper(), Type.FLOAT, _lineIndex, _charIndex)); //TODO throw error about unexpected '.'
                            continue;
                        }


                    }
                    if (hasDecimal)
                    {
                        tokenList.Add(new Token(str.ToUpper(), Type.FLOAT, _lineIndex, _charIndex));
                        continue;
                    }
                    tokenList.Add(new Token(str.ToUpper(), Type.INTEGER, _lineIndex, _charIndex));
                    continue;
                }
                if (token == '/' && (char)_reader.Peek() == '/')
                {
                    // single line comment
                    while (token != '\n')
                    {
                        token = getNextToken();
                    }
                    continue; // get the next token after the comment
                }
                if (token == '/' && (char)_reader.Peek() == '*')
                {
                    // block comment
                    token = getNextToken(); //token = *
                    var level = 1;
                    do
                    {
                        token = getNextToken();
                        if (token == '*' && (char)_reader.Peek() == '/')
                        {
                            level--;
                            getNextToken();
                            continue;
                        }
                        if (token == '/' && (char)_reader.Peek() == '*')
                        {
                            level++;
                        }
                    }
                    while (level > 0);
                    continue; // get the next token after the comment
                }
                if (token == '"')
                {
                    // string
                    var str = "";
                    var valid = true;
                    do
                    {
                        token = getNextToken();
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
                    tokenList.Add(new Token(str.ToUpper(), Type.STRING, _lineIndex, _charIndex));
                    continue;
                }
                if (token == '\'')
                {
                    // char 
                    if ((char)_reader.Peek() == '\'')
                    {
                        getNextToken();
                        tokenList.Add(new Token("", Type.CHAR, _lineIndex, _charIndex)); ;
                    }
                    token = getNextToken();
                    if ((char)_reader.Peek() != '\'')
                    {
                        throw new Exception("Multi-character chars are not allowed");
                    }
                    tokenList.Add(new Token(token.ToString().ToUpper(), Type.CHAR, _lineIndex, _charIndex));
                    continue;
                }
                tokenList.Add(new Token(token.ToString().ToUpper(), Type.UNKNOWN, _lineIndex, _charIndex));
                continue;
            }

            for (int i = tokenList.Count - 2; i >= 0; i--)
            {
                _stack.Push(tokenList[i]);
            }
        }


        public char getNextToken() {
            var token = (char)_reader.Read();
            
            if (token == '\n')
            {
                FileLines.Add(_currentLine);
                _currentLine = "";
                _charIndex = -1;
                _lineIndex += 1;
            }
            else 
            {
                _currentLine += token.ToString();
                _charIndex += 1;
            }
            //Console.WriteLine($"Line: {_lineIndex}, Char: {_charIndex} - {token}");
            return token;
        }
    }
}