using System;
using System.Collections.Generic;

namespace BnfCompiler
{
    public enum ErrorType2 {
        MISMATCHED
    }

    public class ParserResult2 {
        public bool Success = true;
        public List<string> Messages;
        private List<string> FileLines;

        public Token currentToken;

        public ParserResult2(List<string> fileLines) {
            Messages = new List<string>();
            FileLines = fileLines;
        }

        public void AddWarningMessage(int lineIndex, Token token, string message) 
        {
            var desciption = $"Line {token.LineIndex + 1}, Column {token.CharIndex + 1} - Warning: {message}";
            AddParseMessage(desciption, lineIndex, token);
        }

        public void AddErrorMessage(int lineIndex, Token token, string message)
        {
            Success = false;
            var desciption = $"Line {token.LineIndex + 1}, Column {token.CharIndex + 1} - Error: {message}";
            AddParseMessage(desciption, lineIndex, token);
        }

        public void AddPlainErrorMessage(string message)
        {
            Messages.Add(message);
        }

        public void AddParseMessage(string description, int lineIndex, Token token)
        {
            Messages.Add(description);
            Messages.Add(FileLines[lineIndex + 1].Replace('\t', ' '));
            Messages.Add($"{GetFillerString(token.CharIndex, " ")}{GetFillerString(token.Value.Length, "^")}\n");
        }

        private string GetFillerString(int length, string filler) {
            var str = "";
            for (int i = 0; i < length; i++) 
            {
                str += filler;
            }
            return str;
        }
    }

    public class Parser2
    {
        private Scanner _scanner;
        private List<string> ValidTypeMarks = new List<string> { "INTEGER", "FLOAT", "STRING", "BOOL", "CHAR" };
        private ParserResult _result;

        public Parser2(string file)
        {
            _scanner = new Scanner(file);
            _result = new ParserResult(_scanner.FileLines);
        }

        public ParserResult Parse()
        {
            var result = ParseProgram();
            return _result;
        }

        // calls the functions to parse the program header and program body
        bool ParseProgram()
        {
            //Console.WriteLine("--------------------------------Parsing Program");
            var result = ParseProgramHeader();
            result = ParseProgramBody();
            var token = _scanner._stack.Pop();
            if (token == null) 
            {
                _result.AddPlainErrorMessage("Unexpected end of file");
                return false;
            }
            if (token.Value != ".")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Missing end period");
                return false;
            }

            return result;
        }

        // checks for PROGRAM, an identifier, and IS
        bool ParseProgramHeader() 
        {
            //Console.WriteLine("--------------------------------Parsing Program Header");
            var token = _scanner._stack.Pop();
            if (token.Value != "PROGRAM")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Program does not start with 'program' keyword");
                return false;
            }

            token = _scanner._stack.Pop();
            if (token.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(token.LineIndex, token, "Program Value should be an identifier");
                return false;
            }

            token = _scanner._stack.Pop();
            if (token.Value != "IS")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected 'is' but did not receive it");
                return false;
            }

            return true;
        }

        // parses the program body
        bool ParseProgramBody()
        {
            //Console.WriteLine("--------------------------------Parsing Program Body");
            var token = _scanner._stack.Pop();
            while (token.Value != "BEGIN")
            {
                _scanner._stack.Push(token);
                ParseDeclaration();
                token = _scanner._stack.Pop();
                if (token == null) 
                {
                    _result.AddPlainErrorMessage("Unexpected end of file");
                    return false;
                }
            }

            // parse statments in the program body
            while (ParseStatement())
            {
                // do nothing
            }

            token = _scanner._stack.Pop();
            if (token.Value != "END") {
                _result.AddErrorMessage(token.LineIndex, token, "Expected 'end' but did not recieve it");
                return false;
            }

            token = _scanner._stack.Pop();
            if (token.Value != "PROGRAM")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected 'program' but did not recieve it");
                return false;
            }

            return true;
        }

        // parses declarations
        bool ParseDeclaration()
        {
            //Console.WriteLine("--------------------------------Parsing Declaration");
            var token = _scanner._stack.Pop();
            if (token.Value == "GLOBAL")
            {
                //TODO set scope here
            }
            else 
            {
                _scanner._stack.Push(token);
            }
            
            token = _scanner._stack.Pop();
            _scanner._stack.Push(token);
            //Console.Write($"-----------------------------------------------------{token.Value}");
            if (token.Value == "PROCEDURE")
            {
                //parse variable declaration
                //Console.WriteLine(": Procedure");
                ParseProcedureDeclaration();
            }
            else 
            {
                //parse procedure declaration
                //Console.WriteLine(": Variable");
                ParseVariableDeclaration();
            }

            token = _scanner._stack.Pop();
            if (token == null) 
            {
                _result.AddPlainErrorMessage("Unexpected End of File");
                return false;
            }

            if (token.Value != ";")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected ';' but did not see it");
                return false;
            }

            return true;
        }

        // parses variable declarations
        bool ParseVariableDeclaration()
        {
            //Console.WriteLine("--------------------------------Parsing Variable Declaration");
            var typeMark = _scanner._stack.Pop();
            var variableValue = _scanner._stack.Pop();
            if (variableValue == null)
            {
                _result.AddPlainErrorMessage("Unexpected end of file");
                return false;
            }

            //variable Value
            if (variableValue.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(variableValue.LineIndex, variableValue, "Did not see expected identifier");
                return false;
            }

            // array bounds
            var token = _scanner._stack.Pop();
            if (token.Value == "[")
            {
                var lowerBound = _scanner._stack.Pop();
                if (lowerBound.Type != Type.INTEGER && lowerBound.Type != Type.FLOAT)
                {
                    _result.AddErrorMessage(token.LineIndex, token, "Expected integer for lower bound");
                    return false;
                }
                var colon = _scanner._stack.Pop();
                if (colon.Value != ":")
                {
                    _result.AddErrorMessage(token.LineIndex, token, "Exepected colon between lower and upper bounds");
                    return false;
                }
                var upperBound = _scanner._stack.Pop();
                if (upperBound.Type != Type.INTEGER && upperBound.Type != Type.FLOAT)
                {
                    _result.AddErrorMessage(token.LineIndex, token, "Expected integer for upper bound");
                    return false;
                }
                var rightBracketToken = _scanner._stack.Pop();
                if (rightBracketToken.Value != "]")
                {
                    _result.AddErrorMessage(rightBracketToken.LineIndex, rightBracketToken, "Expected ']' but did not see it");
                    return false;
                }
                return true;
            }

            _scanner._stack.Push(token);
            return true;
        }

        // parses procedure declarations
        bool ParseProcedureDeclaration() 
        {
            //Console.WriteLine("--------------------------------Parsing Procedure Declaration");
            // TODO set scope

            ParseProcedureHeader();
            ParseProcedureBody();
            return true;
        }

        bool ParseProcedureHeader() 
        {
            //Console.WriteLine("--------------------------------Parsing Header");
            var token = _scanner._stack.Pop();
            if (token.Value != "PROCEDURE")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected 'procedure' but did not see it");
                return false;
            }
            var identifierToken = _scanner._stack.Pop();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected identifier but did not receive it");
                return false;
            }
            var leftParenToken = _scanner._stack.Pop();
            if (leftParenToken.Value != "(")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected '(' but did not see it");
                return false;
            }
            var rightParenToken = _scanner._stack.Pop();
            if (rightParenToken.Value != ")")
            {
                _scanner._stack.Push(rightParenToken);
                ParseParameterList();
                rightParenToken = _scanner._stack.Pop();
                if (rightParenToken.Value != ")")
                {
                    _result.AddErrorMessage(token.LineIndex, token, "Expected ')' but did not see it");
                    return false;
                }
            }
            return true;
        }

        bool ParseProcedureBody() 
        {
            //Console.WriteLine("--------------------------------Parsing Procedure Body");
            var token = _scanner._stack.Pop();
            while (token.Value != "BEGIN")
            {
                _scanner._stack.Push(token);
                ParseDeclaration();
                token = _scanner._stack.Pop();
                if (token == null)
                {
                    break;
                }
            }

            if (token == null)
            {
                _result.AddPlainErrorMessage("Unexpected end of file");
                return false;
            }

            if (token.Value != "BEGIN")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected 'BEGIN' but did not receive it");
                return false;
            }

            // parse statments in the procedure
            while (ParseStatement())
            {
                // do nothing
            }
            //Console.WriteLine("*******************************");
            token = _scanner._stack.Pop();
            if (token.Value != "END") {
                _result.AddErrorMessage(token.LineIndex, token, "Expected 'end' but did not recieve it");
                return false;
            }

            token = _scanner._stack.Pop();
            if (token.Value != "PROCEDURE")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected 'procedure' but did not recieve it");
                return false;
            }
                        
            return true;
        }

        bool ParseParameterList()
        {
            //Console.WriteLine("--------------------------------Parsing Parameter List");
            ParseParameter();
            var token = _scanner._stack.Pop();
            while(token.Value == ",")
            {
                ParseParameter();
                token = _scanner._stack.Pop();
            }
            
            _scanner._stack.Push(token);
            return true;
        }

        bool ParseParameter() 
        {
            //Console.WriteLine("--------------------------------Parsing Parameter");
            var result = ParseVariableDeclaration();
            if (result == false) {
                return false;
            }

            var variableAccessToken = _scanner._stack.Pop();
            if (variableAccessToken.Value == "IN")
            {
                // do something
                return true;
            }
            else if (variableAccessToken.Value == "OUT")
            {
                // do something
                return true;
            }
            else if (variableAccessToken.Value == "INOUT")
            {
                // do something
                return true;
            }
            else 
            {
                _result.AddErrorMessage(variableAccessToken.LineIndex, variableAccessToken, "Expected one of the following strings but did not see it ('in', 'out', 'inout')");
                return false;
            }
        }

        bool ParseStatement() 
        {
            //Console.WriteLine("--------------------------------Parsing Statement");
            var token = _scanner._stack.Pop();
            if (token.Value == "END")
            {
                _scanner._stack.Push(token);
                return false; // not an error, just an invalid statement
            }

            if (token.Value == "IF"|| token.Value == "FOR" || token.Value == "RETURN")
            {
                _scanner._stack.Push(token);
                switch (token.Value) 
                {
                    case "IF":
                        ParseIfStatement();
                        break;
                    case "FOR":
                        ParseLoopStatement();
                        break;
                    case "RETURN":
                        ParseReturnStatement();
                        break;
                }
            }
            else 
            {
                var nextToken = _scanner._stack.Pop();
                _scanner._stack.Push(nextToken);
                _scanner._stack.Push(token);
                if (nextToken.Value == ":") 
                {
                    ParseAssignmentStatement();
                }
                else if (nextToken.Value == "(")
                {
                    ParseProcedureCall();
                }
                else 
                {
                    _result.AddErrorMessage(token.LineIndex, token, "Failed to parse statement");
                    return false; // not a statement
                }
            }

            token = _scanner._stack.Pop();
            if (token.Value != ";")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected ';' but did not see it");
                return false;
            }

            return true;
        }

        bool ParseAssignmentStatement()
        {
            //Console.WriteLine("--------------------------------Parsing Assignment Statement");
            ParseDestination();
            var token = _scanner._stack.Pop();
            if (token.Value != ":")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected assignment statement to start with ':'");
                return false;
            }
            token = _scanner._stack.Pop();
            if (token.Value != "=")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected assignment statement to include '='");
                return false;
            }
            ParseExpression();
            return true;
        }

        bool ParseIfStatement()
        {
            //Console.WriteLine("--------------------------------Parsing If Statement");
            var token = _scanner._stack.Pop();
            if (token.Value != "IF") 
            {
                _result.AddErrorMessage(token.LineIndex, token, "Invalid if expression");
                return false;
            }

            token = _scanner._stack.Pop();
            if (token.Value != "(") 
            {
                _result.AddErrorMessage(token.LineIndex, token, "Missing left paren in if statement");
                return false;
            }

            ParseExpression();

            token = _scanner._stack.Pop();
            if (token.Value != ")")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Missing right paren in if statement");
                return false;
            }

            token = _scanner._stack.Pop();
            if (token.Value != "THEN")
            {
                _result.AddErrorMessage(token.LineIndex, token, "If expression missing 'then'");
                return false;
            }

            token = _scanner._stack.Pop();
            while (token.Value != "ELSE")
            {
                // do nothing
                _scanner._stack.Push(token);
                if (!ParseStatement())
                {
                    _result.AddPlainErrorMessage("Failed to parse statement");
                    return false;
                }
                token = _scanner._stack.Pop();

            }

            if (token.Value == "ELSE")
            {
                // parse statments in the else block
                while (ParseStatement())
                {
                    // do nothing
                }
                token = _scanner._stack.Pop();
            }
            if (token.Value != "END") 
            {
                _result.AddErrorMessage(token.LineIndex, token, "Expected end of if statement");
                return false;
            }

            token = _scanner._stack.Pop();
            if (token.Value != "IF")
            {
                _result.AddErrorMessage(token.LineIndex, token, "If statement not terminated correctly");
                return false;
            }

            return true;
        }

        bool ParseLoopStatement()
        {
            //Console.WriteLine("--------------------------------Parsing Loop Statement");
            var token = _scanner._stack.Pop();
            if (token.Value != "FOR")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Missing 'for' in for statement");
                return false;
            }
            
            token = _scanner._stack.Pop();
            if (token.Value != "(") 
            {
                _result.AddErrorMessage(token.LineIndex, token, "Missing left paren in for statement");
                return false;
            }

            ParseAssignmentStatement();

            token = _scanner._stack.Pop();
            if (token.Value != ";")
            {
                _result.AddErrorMessage(token.LineIndex, token, "missing semi colon after for statement assignment statement");
                return false;
            }

            ParseExpression();

            token = _scanner._stack.Pop();
            if (token.Value != ")")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Missing right paren in for statement");
                return false;
            }

            // parse statments in the loop block
            while (ParseStatement())
            {
                // do nothing
            }

            token = _scanner._stack.Pop();
            if (token.Value != "END")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Did not see expected end of for loop");
                return false;
            }

            token = _scanner._stack.Pop();
            if (token.Value != "FOR")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Invalid end of for statement");
                return false;
            }

            return true;
        }

        bool ParseReturnStatement()
        {
            //Console.WriteLine("--------------------------------Parsing Return Statement");
            var token = _scanner._stack.Pop();
            if (token.Value != "RETURN")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Invalid return statement");
                return false;
            }
            return true;
        }

        bool ParseProcedureCall()
        {
            //Console.WriteLine("--------------------------------Parsing Procedure Call");

            var token = _scanner._stack.Pop();
            if (token.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(token.LineIndex, token, "Procedure call must start with identifier");
                return false;
            }

            token = _scanner._stack.Pop();
            if (token.Value != "(")
            {
                _result.AddErrorMessage(token.LineIndex, token, "Procedure call statement must include left paren");
                return false;
            }

            token = _scanner._stack.Pop();
            if (token.Value != ")")
            {
                ParseArgumentList();
                token = _scanner._stack.Pop();
            }
            if (token.Value != ")") 
            {
                _result.AddErrorMessage(token.LineIndex, token, "Procedure call must include a right paren");
                return false;
            }

            return true;
        }

        bool ParseDestination() 
        {
            //Console.WriteLine("--------------------------------Parsing Destination");
            var identifierToken = _scanner._stack.Pop();
            if (identifierToken.Type != Type.IDENTIFIER) 
            {
                _result.AddErrorMessage(identifierToken.LineIndex, identifierToken, "Expected identifier but did not get one");
                return false;
            }
            var token = _scanner._stack.Pop();
            if (token.Value == "[")
            {
                ParseExpression();
                token = _scanner._stack.Pop();
                if (token.Value != "]")
                {
                    _result.AddErrorMessage(identifierToken.LineIndex, identifierToken, "Expected ']' but did not see it");
                    return false;
                }
            }
            else 
            {
                _scanner._stack.Push(token);
            }
            
            return true;
        }

        bool ParseArgumentList()
        {
            //Console.WriteLine("--------------------------------Parsing Argument List");
            ParseExpression();
            
            var token = _scanner._stack.Pop();
            while (token.Value == ",")
            {
                _scanner._stack.Push(token);
                ParseExpression();
                token = _scanner._stack.Pop();
            }

            _scanner._stack.Push(token);
            return true;
        }

        // bool ParseExpression()
        // {
        //     Console.WriteLine("--------------------------------Parsing Expression");
        //     if (ParseFactor()) 
        //     { 
        //         var token = _scanner._stack.Pop();
        //         if (token.Value == "&") 
        //         {
        //             return ParseArithmeticOperation();
        //         }
        //         if (token.Value == "|")
        //         {
        //             return ParseArithmeticOperation();
        //         }
        //         _scanner._stack.Push(token);
        //         return ParseArithmeticOperation();
        //     }
        //     return true;
        // }

        // bool ParseArithmeticOperation()
        // {
        //     Console.WriteLine("--------------------------------Parsing Arithmetic Operation");
        //     if (ParseFactor()) 
        //     { 
        //         var token = _scanner._stack.Pop();
        //         if (token.Value == "+") 
        //         {
        //             return ParseRelation();
        //         }
        //         if (token.Value == "-")
        //         {
        //             return ParseRelation();
        //         }
        //         _scanner._stack.Push(token);
        //         return ParseRelation();
        //     }
        //     return true;
        // }

        // bool ParseRelation()
        // {
        //     Console.WriteLine("--------------------------------Parsing Relation");
        //     if (ParseFactor()) 
        //     {
        //         var token = _scanner._stack.Pop();
        //         if (token.Value == "<") 
        //         {
        //             token = _scanner._stack.Pop();
        //             if (token.Value != "=")
        //             {
        //                 _scanner._stack.Push(token);
        //             }
        //             return ParseTerm();
        //         }
        //         if (token.Value == ">")
        //         {
        //             token = _scanner._stack.Pop();
        //             if (token.Value != "=")
        //             {
        //                 _scanner._stack.Push(token);
        //             }
        //             return ParseTerm();
        //         }
        //         if (token.Value == "=")
        //         {
        //             token = _scanner._stack.Pop();
        //             if (token.Value != "=")
        //             {
        //                 _result.AddErrorMessage(token.LineIndex, token, "Expected '=' to follow '='");
        //                 return false;
        //             }
        //             return ParseTerm();
        //         }
        //         if (token.Value == "!")
        //         {
        //             token = _scanner._stack.Pop();
        //             if (token.Value != "=")
        //             {
        //                 _result.AddErrorMessage(token.LineIndex, token, "Expected '=' to follow '!'");
        //                 return false;
        //             }
        //             return ParseTerm();
        //         }
        //         _scanner._stack.Push(token);
        //         return ParseTerm();
        //     }
        //     return true;
        // }

        // bool ParseTerm()
        // {
        //     Console.WriteLine("--------------------------------Parsing Term");
        //     if (ParseFactor()) 
        //     { 
        //         var token = _scanner._stack.Pop();
        //         if (token.Value == "*") 
        //         {
        //             return ParseFactor();
        //         }
        //         if (token.Value == "/")
        //         {
        //             return ParseFactor();
        //         }
        //         _scanner._stack.Push(token);
        //         return ParseFactor();
        //     }
        //     return true;
        // }

        // bool ParseFactor() {
        //     Console.WriteLine("--------------------------------Parsing Factor");
        //     var token = _scanner._stack.Pop();
        //     if (token.Value == "(")
        //     {
        //         ParseExpression();
        //         token = _scanner._stack.Pop();
        //         if (token.Value != ")")
        //         {
        //             _result.AddErrorMessage(token.LineIndex, token, "Expected ')' after expression");
        //             return false;
        //         }
        //         return true;
        //     }
        //     if (token.Value == "NOT")
        //     {
        //         // next should be an arithOp
        //         return ParseArithmeticOperation();
        //     }
        //     if (token.Value == "TRUE")
        //     {
        //         // factor -> boolean literal
        //         return true;
        //     }
        //     if (token.Value == "FALSE")
        //     {
        //         // factor -> boolean literal
        //         return true;
        //     }
        //     if (token.Type == Type.STRING)
        //     {
        //         // factor -> string
        //         return true;
        //     }
        //     if (token.Type == Type.CHAR)
        //     {
        //         // factor -> string
        //         return true;
        //     }
        //     if (token.Type == Type.FLOAT || token.Type == Type.INTEGER)
        //     {
        //         // factor -> number
        //         return true;
        //     }
        //     if (token.Type == Type.IDENTIFIER) {
        //         // factor -> name
        //         _scanner._stack.Push(token);
        //         return ParseName();
        //     }

        //     _scanner._stack.Push(token);
        //     return false;
        // }

        public bool ParseExpression()
        {
            //Console.WriteLine("--------------------------------Parsing Expression");
            var token = _scanner._stack.Pop();
            if (token.Value == "NOT")
            {
                return ParseArithmeticOperation();
            }
            else 
            {
                _scanner._stack.Push(token);
                if (!ParseArithmeticOperation())
                {
                    _result.AddErrorMessage(token.LineIndex, token, "Failed to parse arithmetic operation");
                    return false;
                }

                token = _scanner._stack.Pop();
                if (token == null) 
                {
                    return true;
                }

                if (token.Value == "&" || token.Value == "|")
                {
                    return ParseArithmeticOperation();
                } 
                else 
                {
                    _scanner._stack.Push(token);
                    return true;
                }
            }
        }

        public bool ParseArithmeticOperation()
        {
            //Console.WriteLine("--------------------------------Parsing Arithmetic Operation");
            var token1 = _scanner._stack.Pop();
            var token2 = _scanner._stack.Pop();
            _scanner._stack.Push(token2);
            _scanner._stack.Push(token1);
            
            if (token1 == null || token2 == null)
            {
                _result.AddPlainErrorMessage("Unexpected end of input");
                return false;
            }

            if (token2.Value == "+")
            {
                if (!ParseRelation())
                {
                    _result.AddErrorMessage(token2.LineIndex, token2, "Failed to parse arithmetic operation");
                    return false;
                }
                _scanner._stack.Pop(); // should be +
                return ParseRelation();
            }
            else if (token2.Value == "-")
            {
                if (!ParseRelation())
                {
                    _result.AddPlainErrorMessage("Could not parse relation");
                    return false;
                }
                 _scanner._stack.Pop(); // should be -
                return ParseRelation();
            }
            else
            {
                return ParseRelation();
            }
        }

        public bool ParseRelation()
        {
            //Console.WriteLine("--------------------------------Parsing Relation");
            var token1 = _scanner._stack.Pop();
            var token2 = _scanner._stack.Pop();
            _scanner._stack.Push(token2);
            _scanner._stack.Push(token1);
            
            if (token1 == null || token2 == null)
            {
                 _result.AddPlainErrorMessage("Unexpected end of input");
                return false;
            }

            if (token2.Value == "<")
            {
                if (!ParseTerm())
                {
                    _result.AddPlainErrorMessage("Could not parse term");
                    return false;
                }
                _scanner._stack.Pop(); // should be <
                var token = _scanner._stack.Pop();
                if (token.Value != "=")
                {
                    _scanner._stack.Push(token);
                }
                return ParseTerm();
            }
            else if (token2.Value == ">")
            {
                if (!ParseTerm())
                {
                    _result.AddPlainErrorMessage("Could not parse term");
                    return false;
                }
                _scanner._stack.Pop(); // should be >
                var token = _scanner._stack.Pop();
                if (token.Value != "=")
                {
                    _scanner._stack.Push(token);
                }
                return ParseTerm();
            }
            else if (token2.Value == "=")
            {
                if (!ParseTerm())
                {
                    _result.AddPlainErrorMessage("Could not parse term");
                    return false;
                }
                _scanner._stack.Pop(); // should be =
                var token = _scanner._stack.Pop();
                if (token.Value != "=")
                {
                    _scanner._stack.Push(token);
                }
                return ParseTerm();
            }
            else if (token2.Value == "!")
            {
                if (!ParseTerm())
                {
                    _result.AddPlainErrorMessage("Could not parse term");
                    return false;
                }
                _scanner._stack.Pop(); // should be !
                var token = _scanner._stack.Pop();
                if (token.Value != "=")
                {
                    _result.AddErrorMessage(token.LineIndex, token, "Error: '!' should be followed by '='");
                    return false;
                }
                return ParseTerm();
            }
            else
            {
                return ParseTerm();
            }
        }

        public bool ParseTerm()
        {
            //Console.WriteLine("--------------------------------Parsing Term");
            var token1 = _scanner._stack.Pop();
            var token2 = _scanner._stack.Pop();
            _scanner._stack.Push(token2);
            _scanner._stack.Push(token1);
            
            if (token1 == null || token2 == null)
            {
                 _result.AddPlainErrorMessage("Unexpected end of input");
                return false;
            }

            if (token2.Value == "*")
            {
                if (!ParseFactor())
                {
                    _result.AddPlainErrorMessage("Error: Could not parse factor");
                    return false;
                }
                Console.WriteLine("* == " + _scanner._stack.Pop()); // should be *
                return ParseFactor();
            }
            else if (token2.Value == "/")
            {
                if (!ParseFactor())
                {
                    _result.AddPlainErrorMessage("Error: Could not parse factor");
                    return false;
                }
                Console.WriteLine("/ == " + _scanner._stack.Pop()); // should be /
                return ParseFactor();
            }
            else
            {
                return ParseFactor();
            }
        }

        public bool ParseFactor()
        {
            //Console.WriteLine("--------------------------------Parsing Factor");
            var token = _scanner._stack.Pop();
            if (token == null)
            {
                _result.AddPlainErrorMessage("Unexpected end of input");
                return false;
            }

            if (token.Value == "(")
            {
                if (!ParseExpression())
                {
                    _result.AddPlainErrorMessage("Error: Could not parse expression");
                    return false;
                }
                token = _scanner._stack.Pop();
                if (token == null) {
                    _result.AddPlainErrorMessage("Error: Unexpected end of input");
                    return false;
                }
                if (token.Value != ")")
                {
                    _result.AddErrorMessage(token.LineIndex, token, "Expected to see right paren");
                    return false;
                }
                return true;
            }
            else if (token.Value == "TRUE")
            {
                return true;
            }
            else if (token.Value == "FALSE")
            {
                return true;
            }
            else if (token.Value == "-")
            {
                token = _scanner._stack.Pop();
                if (token.Type == Type.FLOAT || token.Type == Type.INTEGER)
                {
                    return true;
                }
                return ParseName();
            }
            else if (token.Type == Type.FLOAT || token.Type == Type.INTEGER)
            {
                return true;
            }
            else if (token.Type == Type.STRING || token.Type == Type.CHAR)
            {
                return true;
            }
            else if (token.Type == Type.IDENTIFIER)
            {
                _scanner._stack.Push(token);
                return ParseName();
            }

            _result.AddErrorMessage(token.LineIndex, token, "What the hell is this token?");
            return false;
        }

        bool ParseName() {
            //Console.WriteLine("--------------------------------Parsing Name");
            var token = _scanner._stack.Pop(); // we know this is an identifier
            token = _scanner._stack.Pop();
            if (token.Value == "[")
            {
                ParseExpression();
                token = _scanner._stack.Pop();
                if (token.Value != "]")
                {   
                    _result.AddErrorMessage(token.LineIndex, token, "Expected ']' after name with index");
                    return false;
                }
            }
            else 
            {
                _scanner._stack.Push(token);
            }

            return true;
        }
    }
}