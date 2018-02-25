using System;
using System.Collections.Generic;
using System.IO;

namespace BnfCompiler
{
    public class Scanner
    {
        private List<string> Keywords = new List<string>() { "PROGRAM", "IS", "BEGIN", "END", "GLOBAL", "PROCEDURE", "IN", "OUT", "INOUT", "INTEGER", "FLOAT", "STRING", "CHAR", "BOOL", "TRUE", "FALSE", "IF", "THEN", "ELSE", "FOR", "RETURN", "NOT" };
        private List<string> Specials = new List<string>() { ";", "\"", "'", "[", "]", ".", "(", ")", ",", ":", ":=", "==", "&", "|", "<", "<=", ">", ">=", "!=", "*", "/", "-", "+" };
        public List<string> FileLines = new List<string>();

        private List<Token> tokenList;
        private int commentLevel = 0;

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
                FileLines.Add(lines[i]);

                var line = lines[i].Replace('\t', ' ').Trim();

                var words = line.ToUpper().Split(' ');

                foreach (var word in words)
                {
                    Console.Write($"Lexing: {word} -> ");
                    var type = LexWord(word, true);
                    if (type == Type.UNKNOWN)
                    {
                        Console.WriteLine($"\nWARNING: '{word}' unknown, falling back to char by char");
                        LexCharacterByCharacter(word);
                        Console.WriteLine();
                    }
                    else if (type == Type.LINE_COMMENT)
                    {
                        i += 1;
                        break;
                    }
                    else if (type == Type.BLOCK_COMMENT)
                    {
                        Console.WriteLine($"Comment Level: {commentLevel}");
                    }
                }
            }

            Console.WriteLine($"Final comment level (should be 0): {commentLevel}");

            Console.WriteLine("\n\n\nTokens:");

            foreach(var token in tokenList)
            {
                Console.WriteLine($"{token.Value} -> {Enum.GetName(typeof(Type), token.Type)}");
            }

            for (var i = tokenList.Count - 1; i >= 0; i--)
            {
                Stack.Push(tokenList[i]);
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
            else if (word.StartsWith("\"") && word.EndsWith("\""))
            {
                // char
                type = Type.CHAR;
            }
            else if (Int32.TryParse(word, out tempInt))
            {
                // integer
                type = Type.INTEGER;
            }
            else if (float.TryParse(word, out tempFloat))
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
                if (type != Type.WHITESPACE && type != Type.UNKNOWN && type != Type.BLOCK_COMMENT && type != Type.LINE_COMMENT && commentLevel == 0)
                {
                    tokenList.Add(new Token(word, type, 0, 0));
                }

                Console.WriteLine(Enum.GetName(typeof(Type), type));
            }

            return type;
        }

        public bool validIdentifier(string word)
        {
            string allowableLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_";

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

            var type = LexWord(word.Substring(0, i));
            while (type == Type.UNKNOWN)
            {
                if (i - 1 == 0) 
                {
                    Console.Write($"Lexing: {word.Substring(0, i)} -> ");
                    LexWord(word.Substring(0, i), true);
                    return;
                }

                i -= 1;
                type = LexWord(word.Substring(0, i));
            }

            Console.Write($"Lexing: {word.Substring(0, i)} -> ");
            LexWord(word.Substring(0, i), true);

            if (type == Type.BLOCK_COMMENT)
            {
                Console.WriteLine($"Comment Level: {commentLevel}");
            }

            LexCharacterByCharacter(word.Substring(i));
        }
    }
}