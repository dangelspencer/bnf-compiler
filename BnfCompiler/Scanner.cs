using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BnfCompiler
{
    public class Scanner
    {
        private List<string> Keywords = new List<string>() { "PROGRAM", "IS", "BEGIN", "END", "GLOBAL", "PROCEDURE", "IN", "OUT", "INOUT", "INTEGER", "FLOAT", "STRING", "CHAR", "BOOL", "IF", "THEN", "ELSE", "FOR", "RETURN", "NOT" };
        private List<string> Specials = new List<string>() { ";", "\"", "'", "[", "]", ".", "(", ")", ",", ":", ":=", "==", "&", "|", "<", "<=", ">", ">=", "!=", "*", "/", "-", "+" };
        public List<string> FileLines = new List<string>();

        private List<Token> tokenList;
        private int commentLevel = 0;
        private int currentLine;
        private bool isLineComment = false;

        public Stack<Token> Stack;
        public Scanner(string file)
        {
            FileStream fileStream = new FileStream(file, FileMode.Open);
            var _reader = new StreamReader(fileStream);
            Stack = new Stack<Token>();

            tokenList = new List<Token>();

            var fileContents = _reader.ReadToEnd();
            var lines = fileContents.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                currentLine = i;

                var line = lines[i].Replace('\t', ' ').Trim();
                FileLines.Add(line);
                isLineComment = false;
                
                Console.WriteLine($"\nNEXT LINE: {line.ToUpper()}");
                LexCharacterByCharacter(line.ToUpper());
            }

            Console.WriteLine($"Final comment level (should be 0): {commentLevel}");

            Console.WriteLine("\n\n\nTokens:");

            foreach(var token in tokenList)
            {
                Console.Write($"{token.Value} -> {Enum.GetName(typeof(Type), token.Type)} ({token.LineIndex}, {token.CharIndex})");
                Console.WriteLine(GetTypeValue(token));
            }
            Console.WriteLine("\n\n\n");

            for (var i = tokenList.Count - 1; i >= 0; i--)
            {
                Stack.Push(tokenList[i]);
            }
        }

        public string GetTypeValue(Token token)
        {
            if (token.Type == Type.KEYWORD)
            {
                return $" KEYWORD: {Enum.GetName(typeof(Keyword), token.KeywordValue)}";
            }
            else if (token.Type == Type.SPECIAL)
            {
                return $" SPECIAL: {Enum.GetName(typeof(Special), token.SpecialValue)}";
            }
            else
            {
                return "";
            }
        }

        public Type LexWord(string word, bool createToken = false)
        {
            float tempFloat = 0;
            int tempInt = 0;
            var type = Type.UNKNOWN;
            if (word == "//")
            {
                // opening comment
                type = Type.LINE_COMMENT;
            }
            else if (word == "/*") 
            {
                if (createToken) commentLevel += 1;
                type = Type.BLOCK_COMMENT;
            }
            else if (word == "*/")
            {
                if (createToken) commentLevel -= 1;
                type = Type.BLOCK_COMMENT;
            }
            else if (String.IsNullOrWhiteSpace(word))
            {
                // whitespace
                type = Type.WHITESPACE;
            }
            else if (word == "TRUE" || word == "FALSE")
            {
                type = Type.BOOL;
            }
            else if (Keywords.Contains(word))
            {
                // keyword
                type = Type.KEYWORD;
            }
            else if (Specials.Contains(word))
            {
                // special character
                type = Type.SPECIAL;
            }
            else if (word.StartsWith("\"") && word.EndsWith("\""))
            {
                // string
                type = Type.STRING;
            }
            else if (word.StartsWith("'") && word.EndsWith("'"))
            {
                // char
                type = Type.CHAR;
            }
            else if (Int32.TryParse(word, out tempInt) && !word.Contains("-") && !word.Contains(",") && !word.Contains(" "))
            {
                // integer
                type = Type.INTEGER;
            }
            else if (float.TryParse(word, out tempFloat) && !word.Contains("-") && !word.Contains(",") && !word.Contains(" "))
            {
                // float
                type = Type.FLOAT;
            }
            else if (validIdentifier(word))
            {
                // identifier
                type = Type.IDENTIFIER;
            }

            if (createToken)
            {
                if (type != Type.WHITESPACE && type != Type.UNKNOWN && type != Type.BLOCK_COMMENT && type != Type.LINE_COMMENT && commentLevel == 0 && !isLineComment)
                {
                    var tokensInLine = tokenList.Where(x => x.LineIndex == currentLine).Select(x => x.Value).ToList();
                    var str = String.Join("", tokensInLine);
                    var index = 0;
                    if (str != "")
                    {
                        var line = FileLines[currentLine].Substring(str.Length).ToUpper();
                        index = line.IndexOf(word) + (str.Length);
                    }
                    tokenList.Add(new Token(word, type, currentLine, index)); 
                }
                
                Console.WriteLine(Enum.GetName(typeof(Type), type));
            }

            return type;
        }

        public bool validIdentifier(string word)
        {
            string allowableLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789";

            foreach (char c in word)
            {
                if (!allowableLetters.Contains(c.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        public void LexCharacterByCharacter(string word)
        {
            
            var i = word.Length;
            if (i == 0)
            {
                return;
            }

            Console.WriteLine($"CHECKING: {word.Substring(0, i)}");
            var type = LexWord(word.Substring(0, i));
            while (type == Type.UNKNOWN)
            {
                if (i - 1 == 0) 
                {
                    Console.Write($"Lexing (single char): {word.Substring(0, i)} -> ");
                    LexWord(word.Substring(0, i), true);
                    if (word.Length > 1) 
                    {
                        LexCharacterByCharacter(word.Substring(i));
                    }
                    return;
                }

                i -= 1;
                Console.WriteLine($"CHECKING: {word.Substring(0, i)}");
                type = LexWord(word.Substring(0, i));
            }

            Console.Write($"Lexing: {word.Substring(0, i)} -> ");
            LexWord(word.Substring(0, i), true);

            if (type == Type.BLOCK_COMMENT)
            {
                Console.WriteLine($"Comment Level: {commentLevel}");
            }
            else if (type == Type.LINE_COMMENT)
            {
                isLineComment = true;
                return;
            }
            
            LexCharacterByCharacter(word.Substring(i));
        }
    }
}