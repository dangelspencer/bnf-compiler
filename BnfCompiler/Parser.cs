using System;
using System.Collections.Generic;

namespace BnfCompiler
{
    public enum ErrorType
    {
        MISMATCHED
    }

    public class ParserResult
    {
        public bool Success = true;
        public List<string> Messages;
        private List<string> FileLines;

        public ParserResult(List<string> fileLines)
        {
            Messages = new List<string>();
            FileLines = fileLines;
        }

        public void AddWarningMessage(Token token, string message)
        {
            var desciption = $"Line {token.LineIndex + 1}, Column {token.CharIndex + 1} - Warning: {message}";
            AddParseMessage(desciption, token);
        }

        public void AddErrorMessage(Token token, string message)
        {
            Success = false;
            var desciption = $"Line {token.LineIndex + 1}, Column {token.CharIndex + 1} - Error: {message}";
            AddParseMessage(desciption, token);
        }

        public void AddPlainErrorMessage(string message)
        {
            Messages.Add(message);
        }

        public void AddParseMessage(string description, Token token)
        {
            Messages.Add(description);
            Messages.Add(FileLines[token.LineIndex + 1].Replace('\t', ' '));
            Messages.Add($"{GetFillerString(token.CharIndex, " ")}{GetFillerString(token.Value.Length, "^")}\n");
        }

        private string GetFillerString(int length, string filler)
        {
            var str = "";
            for (int i = 0; i < length; i++)
            {
                str += filler;
            }
            return str;
        }
    }

    public class Parser
    {
        private Scanner _scanner;
        private List<string> ValidTypeMarks = new List<string> { "INTEGER", "FLOAT", "STRING", "BOOL", "CHAR" };
        private ParserResult _result;

        private Stack<Token> Stack;

        public Parser(string file)
        {
            _scanner = new Scanner(file);
            _result = new ParserResult(_scanner.FileLines);
            Stack = _scanner.Stack;
        }

        public ParserResult Parse()
        {
            ParseProgram();
            return _result;
        }

        // calls the functions to parse the program header and program body
        void ParseProgram()
        {
            Console.WriteLine("Parsing: PROGRAM");
            ParseProgramHeader();
        }


        void ProcessToken()
        {
            var token = Stack.Pop();
            // add token to symbol table
        }
        void ParseProgramHeader()
        {
            Console.WriteLine("Parsing: PROGRAM HEADER");
            var programToken = Stack.Peek();
            if (programToken.Type != Type.KEYWORD && programToken.KeywordValue != Keyword.PROGRAM)
            {
                _result.AddErrorMessage(programToken, "Expected 'PROGRAM' keyword");
            }
            else
            {
                ProcessToken();
            }

            var identifierToken = Stack.Peek();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier");
            }
            else 
            {
                ProcessToken();
            }

            var isToken = Stack.Pop();
            if (isToken.Type != Type.KEYWORD && isToken.KeywordValue != Keyword.IS)
            {
                _result.AddErrorMessage(isToken, "Expected 'IS' keyword");
            }
        }

        void ParseProgramBody()
        {
            Console.WriteLine("Parsing: PROGRAM BODY");
            var beginToken = Stack.Pop();
            while (beginToken.Type != Type.KEYWORD && beginToken.KeywordValue != Keyword.BEGIN)
            {
                Stack.Push(beginToken);
                ParseDeclaration();
                beginToken = Stack.Pop();
            }

            var endToken = Stack.Pop();
            while (endToken.Type != Type.KEYWORD && beginToken.KeywordValue != Keyword.END)
            {
                Stack.Push(endToken);
                ParseStatement();
                endToken = Stack.Pop();
            }

            var programToken = Stack.Pop();
            if (programToken.Type != Type.KEYWORD && programToken.KeywordValue != Keyword.PROGRAM)
            {
                _result.AddErrorMessage(programToken, "Expected 'Program' keyword");
            }
        }

        void ParseDeclaration()
        {
            Console.WriteLine("Parsing: DECLARATION");

            var globalToken = Stack.Pop();
            if (globalToken.Type != Type.KEYWORD && globalToken.KeywordValue != Keyword.GLOBAL)
            {
                Stack.Push(globalToken);
            }

            var procedureToken = Stack.Pop();
            Stack.Push(procedureToken);
            if (procedureToken.Type != Type.KEYWORD && procedureToken.KeywordValue != Keyword.PROCEDURE)
            {
                ParseVariableDeclaration();
            }
            else
            {
                ParseProcedureDeclaration();
            }

            var semicolonToken = Stack.Pop();
            if (semicolonToken.Type != Type.SPECIAL && semicolonToken.SpecialValue != Special.SEMICOLON)
            {
                _result.AddWarningMessage(semicolonToken, "Missing semi-colon");
                Stack.Push(semicolonToken);
            }
        }

        void ParseProcedureDeclaration()
        {
            ParseProcedureHeader();
            ParseProcedureBody();
        }

        void ParseProcedureHeader()
        {
            var procedureToken = Stack.Pop();
            if (procedureToken.Type != Type.KEYWORD && procedureToken.KeywordValue != Keyword.PROCEDURE)
            {
                _result.AddErrorMessage(procedureToken, "WTF - this should be a 'Procedure' keyword");
            }

            var identifierToken = Stack.Pop();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier");
            }

            var leftParenToken = Stack.Pop();
            if (leftParenToken.Type != Type.SPECIAL && leftParenToken.SpecialValue != Special.LEFT_PAREN)
            {
                _result.AddErrorMessage(leftParenToken, "Expected open paren");
            }

            var rightParenToken = Stack.Pop();
            if (rightParenToken.Type != Type.SPECIAL && rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                Stack.Push(rightParenToken);
                ParseParameterList();
            }

            rightParenToken = Stack.Pop();
            if (rightParenToken.Type != Type.SPECIAL && rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                _result.AddErrorMessage(rightParenToken, "Expected right paren");
            }
        }

        void ParseParameterList()
        {
            ParseParameter();
            var commaToken = Stack.Pop();
            if (commaToken.Type == Type.SPECIAL && commaToken.SpecialValue == Special.COMMA)
            {
                ParseParameterList();
            }
            else 
            {
                Stack.Push(commaToken);
            }


        }

        void ParseParameter()
        {
            ParseVariableDeclaration();

            var accessToken = Stack.Pop();
            if (accessToken.Type != Type.KEYWORD && !(accessToken.KeywordValue == Keyword.IN || accessToken.KeywordValue == Keyword.OUT || accessToken.KeywordValue == Keyword.INOUT))
            {
                _result.AddErrorMessage(accessToken, "Expected access level");
            }
        }

        void ParseProcedureBody()
        {
            var beginToken = Stack.Pop();
            while (beginToken.Type != Type.KEYWORD && beginToken.KeywordValue != Keyword.BEGIN)
            {
                Stack.Push(beginToken);
                ParseDeclaration();
                beginToken = Stack.Pop();
            }

            var endToken = Stack.Pop();
            while (endToken.Type != Type.KEYWORD && beginToken.KeywordValue != Keyword.END)
            {
                Stack.Push(endToken);
                ParseStatement();
                endToken = Stack.Pop();
            }

            var programToken = Stack.Pop();
            if (programToken.Type != Type.KEYWORD && programToken.KeywordValue != Keyword.PROCEDURE)
            {
                _result.AddErrorMessage(programToken, "Expected 'Procedure' keyword");
            }
        }

        void ParseVariableDeclaration()
        {
            var typeMarkToken = Stack.Pop();
            if (typeMarkToken.Type != Type.KEYWORD && !(typeMarkToken.KeywordValue == Keyword.INTEGER || typeMarkToken.KeywordValue == Keyword.FLOAT || typeMarkToken.KeywordValue == Keyword.STRING || typeMarkToken.KeywordValue == Keyword.BOOL || typeMarkToken.KeywordValue == Keyword.CHAR))
            {
                _result.AddErrorMessage(typeMarkToken, "Expected type mark");
            }

            var identifierToken = Stack.Pop();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier");
            }

            var leftBracketToken = Stack.Pop();
            if (leftBracketToken.Type != Type.SPECIAL && leftBracketToken.SpecialValue != Special.LEFT_BRACKET)
            {
                Stack.Push(leftBracketToken);
                return;
            }

            var negativeToken = Stack.Pop();
            if (negativeToken.Type != Type.SPECIAL && negativeToken.SpecialValue != Special.NEGATIVE)
            {
                Stack.Push(negativeToken);
            }

            var lowerBoundToken = Stack.Pop();
            if (lowerBoundToken.Type != Type.INTEGER)
            {
                _result.AddErrorMessage(lowerBoundToken, "Expected integer");
            }

            var colonToken = Stack.Pop();
            if (colonToken.Type != Type.SPECIAL && colonToken.SpecialValue != Special.COLON)
            {
                _result.AddErrorMessage(colonToken, "Expected colon");
            }

            negativeToken = Stack.Pop();
            if (negativeToken.Type != Type.SPECIAL && negativeToken.SpecialValue != Special.NEGATIVE)
            {
                Stack.Push(negativeToken);
            }

            var upperBoundToken = Stack.Pop();
            if (upperBoundToken.Type != Type.INTEGER)
            {
                _result.AddErrorMessage(upperBoundToken, "Expected integer");
            }

            var rightBracketToken = Stack.Pop();
            if (rightBracketToken.Type != Type.SPECIAL && rightBracketToken.SpecialValue != Special.RIGHT_BRACKET)
            {
                _result.AddErrorMessage(rightBracketToken, "Expected right bracket token");
            }
        }

        void ParseStatement()
        {
            var token = Stack.Pop();
            if (token.Type == Type.KEYWORD && token.KeywordValue == Keyword.RETURN)
            {
                // do nothing
            }
            else if (token.Type == Type.KEYWORD && token.KeywordValue == Keyword.IF)
            {
                Stack.Push(token);
                ParseIfStatement();
            }
            else if (token.Type == Type.KEYWORD && token.KeywordValue == Keyword.FOR)
            {
                Stack.Push(token);
                ParseLoopStatement();
            }
            else if (token.Type == Type.IDENTIFIER) 
            {
                var nextToken = Stack.Peek();
                if (nextToken.Type == Type.SPECIAL && nextToken.SpecialValue == Special.LEFT_PAREN)
                {
                    // procedure call 
                    Stack.Push(token);
                    ParseProcedureCall();
                }
                else 
                {
                    Stack.Push(token);
                    ParseAssignment();
                }
            }

            var semicolonToken = Stack.Pop();
            if (semicolonToken.Type != Type.SPECIAL && semicolonToken.SpecialValue != Special.SEMICOLON)
            {
                _result.AddWarningMessage(semicolonToken, "Missing semi colon");
                Stack.Push(semicolonToken);
            }
        }

        void ParseProcedureCall()
        {
            var identifierToken = Stack.Pop();
            if (identifierToken.Type != Type.IDENTIFIER) 
            {
                _result.AddErrorMessage(identifierToken, "Expected Identifier token");
            }

            var leftParenToken = Stack.Pop();
            if (leftParenToken.Type != Type.SPECIAL && leftParenToken.SpecialValue != Special.LEFT_PAREN)
            {
                _result.AddErrorMessage(leftParenToken, "Expected left paren token");
            }

            ParseArgumentList();

            var rightParenToken = Stack.Pop();
            if (rightParenToken.Type != Type.SPECIAL && rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                _result.AddErrorMessage(rightParenToken, "Expected right paren token");
            }
        }

        void ParseAssignment()
        {
            ParseDestination();

            var equalsToken = Stack.Pop();
            if (equalsToken.Type != Type.SPECIAL && equalsToken.SpecialValue != Special.EQUALS)
            {
                _result.AddErrorMessage(equalsToken, "Expected equals token");
            }

            ParseExpression();
        }

        void ParseDestination()
        {
            var identifierToken = Stack.Pop();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier token");
            }

            var leftBracketToken = Stack.Pop();
            if (leftBracketToken.Type != Type.SPECIAL && leftBracketToken.SpecialValue != Special.LEFT_BRACKET)
            {
                Stack.Push(leftBracketToken);
                return;
            }

            ParseExpression();

            var rightBracketToken = Stack.Pop();
            if (rightBracketToken.Type != Type.SPECIAL && rightBracketToken.SpecialValue != Special.RIGHT_BRACKET)
            {
                _result.AddErrorMessage(rightBracketToken, "Expected right bracket token");
            }
        }

        void ParseIfStatement()
        {
            var ifToken = Stack.Pop();
            if (ifToken.Type != Type.KEYWORD && ifToken.KeywordValue != Keyword.IF)
            {
                _result.AddErrorMessage(ifToken, "WTF? this should be an if token");
            }

            var leftParenToken = Stack.Pop();
            if (leftParenToken.Type != Type.SPECIAL && leftParenToken.SpecialValue != Special.LEFT_PAREN)
            {
                _result.AddErrorMessage(leftParenToken, "Expected left paren token");
            }

            ParseExpression();

            var rightParenToken = Stack.Pop();
            if (rightParenToken.Type != Type.SPECIAL && rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                _result.AddErrorMessage(rightParenToken, "Expected right paren token");
            }

            var thenToken = Stack.Pop();
            if (thenToken.Type != Type.KEYWORD && thenToken.KeywordValue != Keyword.THEN)
            {
                _result.AddErrorMessage(thenToken, "Expected 'then' token");
            }

            var endElseToken = Stack.Pop();
            while (endElseToken.Type != Type.KEYWORD && !(endElseToken.KeywordValue == Keyword.END || endElseToken.KeywordValue == Keyword.ELSE))
            {
                ParseStatement();
                endElseToken = Stack.Pop();
            }

            if (endElseToken.KeywordValue == Keyword.ELSE)
            {
                while (endElseToken.Type != Type.KEYWORD && endElseToken.KeywordValue != Keyword.END)
                {
                    ParseStatement();
                    endElseToken = Stack.Pop();
                }
            }

            ifToken = Stack.Pop();
            if (ifToken.Type != Type.KEYWORD && ifToken.KeywordValue != Keyword.IF)
            {
                _result.AddErrorMessage(ifToken, "Missing end if token");
            }
        }

        void ParseLoopStatement()
        {
            var forToken = Stack.Pop();
            if (forToken.Type != Type.KEYWORD && forToken.KeywordValue != Keyword.FOR)
            {
                _result.AddErrorMessage(forToken, "WTF? This should be a for token");
            }

            var leftParenToken = Stack.Pop();
            if (leftParenToken.Type != Type.SPECIAL && leftParenToken.SpecialValue != Special.LEFT_PAREN)
            {
                _result.AddErrorMessage(leftParenToken, "Expected left paren token");
            }

            ParseAssignment();

            var semicolonToken = Stack.Pop();
            if (semicolonToken.Type != Type.SPECIAL && semicolonToken.SpecialValue != Special.SEMICOLON)
            {
                _result.AddWarningMessage(semicolonToken, "Missing semi colon");
                Stack.Push(semicolonToken);
            }

            ParseExpression();

            var rightParenToken = Stack.Pop();
            if (rightParenToken.Type != Type.SPECIAL && rightParenToken.SpecialValue != Special.RIGHT_BRACKET)
            {
                _result.AddErrorMessage(rightParenToken, "Expected right bracket token");
            }

            var endToken = Stack.Pop();
            while (endToken.Type != Type.KEYWORD && endToken.KeywordValue != Keyword.END)
            {
                ParseStatement();
                endToken = Stack.Pop();
            }

            forToken = Stack.Pop();
            if (forToken.Type != Type.KEYWORD && forToken.KeywordValue != Keyword.FOR)
            {
                _result.AddErrorMessage(forToken, "Missing ending for token");
            } 
        }

        void ParseExpression()
        {
            var negativeToken = Stack.Pop();
            if (negativeToken.Type != Type.KEYWORD && negativeToken.KeywordValue != Keyword.NOT)
            {
                Stack.Push(negativeToken);
                return;
            }

            ParseArithOp();
        }

        void ParseArithOp()
        {
            ParseRelation();

            var andOrToken = Stack.Pop();
            if (andOrToken.Type != Type.SPECIAL && !(andOrToken.SpecialValue == Special.AND || andOrToken.SpecialValue == Special.OR))
            {
                Stack.Push(andOrToken);
                return;
            }

            ParseRelation();
        }

        void ParseRelation()
        {
            ParseTerm();

            var plusNegToken = Stack.Pop();
            if (plusNegToken.Type != Type.SPECIAL && !(plusNegToken.SpecialValue == Special.PLUS || plusNegToken.SpecialValue == Special.NEGATIVE))
            {
                Stack.Push(plusNegToken);
                return;
            }

            ParseTerm();
        }

        void ParseTerm()
        {
            ParseFactor();

            var relationToken = Stack.Pop();
            if (relationToken.Type != Type.SPECIAL && !(relationToken.SpecialValue == Special.LESS_THAN || relationToken.SpecialValue == Special.LESS_THAN_OR_EQUAL || relationToken.SpecialValue == Special.GREATER_THAN || relationToken.SpecialValue == Special.GREATER_THAN_OR_EQUAL || relationToken.SpecialValue == Special.DOUBLE_EQUALS || relationToken.SpecialValue == Special.NOT_EQUAL))
            {
                Stack.Push(relationToken);
                return;
            }

            ParseFactor();
        }

        void ParseFactor()
        {
            ParseWord();

            var multDivToken = Stack.Pop();
            if (multDivToken.Type != Type.SPECIAL && !(multDivToken.SpecialValue == Special.MULTIPLY || multDivToken.SpecialValue == Special.DIVIDE))
            {
                Stack.Push(multDivToken);
                return;
            }

            ParseWord();
        }

        void ParseWord()
        {
            var token = Stack.Pop();
            if (token.Type == Type.SPECIAL && token.SpecialValue == Special.LEFT_PAREN)
            {
                ParseExpression();

                var rightParenToken = Stack.Pop();
                if (rightParenToken.Type != Type.SPECIAL && rightParenToken.SpecialValue != Special.RIGHT_BRACKET)
                {
                    _result.AddErrorMessage(rightParenToken, "Expected right bracket token");
                }
            }
            else if (token.Type == Type.STRING)
            {
                // do nothing
            }
            else if (token.Type == Type.CHAR)
            {
                // do nothing
            }
            else if (token.Type == Type.KEYWORD && token.KeywordValue == Keyword.TRUE)
            {
                // do nothing
            }
            else if (token.Type == Type.KEYWORD && token.KeywordValue == Keyword.FALSE)
            {
                // do nothing
            }
            else if (token.Type == Type.INTEGER || token.Type == Type.FLOAT)
            {
                // do nothing
            }
            else if (token.Type == Type.IDENTIFIER)
            {
                Stack.Push(token);
                ParseName();
            }
            else if (token.Type == Type.SPECIAL && token.SpecialValue == Special.NEGATIVE)
            {
                var nextToken = Stack.Pop();
                if (nextToken.Type == Type.INTEGER || nextToken.Type == Type.FLOAT)
                {
                    // do nothing
                }
                else
                {
                    ParseName();
                }
            }
            else 
            {
                _result.AddErrorMessage(token, "Unexpected token");
            }
        }

        void ParseName()
        {
            var identifierToken = Stack.Pop();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier token");
            }

            var leftParenToken = Stack.Pop();
            if (leftParenToken.Type != Type.SPECIAL && leftParenToken.SpecialValue != Special.LEFT_PAREN)
            {
                Stack.Push(leftParenToken);
                return;
            }

            ParseExpression();

            var rightParenToken = Stack.Pop();
            if (rightParenToken.Type != Type.SPECIAL && rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                _result.AddErrorMessage(rightParenToken, "Expected right paren token");
            }
        }

        void ParseArgumentList()
        {
            var token = Stack.Pop();
            if (token.Type != Type.SPECIAL && token.SpecialValue != Special.RIGHT_PAREN)
            {
                Stack.Push(token);
                return;
            }

            ParseExpression();

            token = Stack.Pop();
            if (token.Type != Type.SPECIAL && token.SpecialValue != Special.COMMA)
            {
                ParseArgumentList();
            }
            else  
            {
                Stack.Push(token);
            }
        }
    }
}