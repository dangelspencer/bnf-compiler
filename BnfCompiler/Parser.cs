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

        private Stack<Token> TokenStack;

        public Parser(string file)
        {
            _scanner = new Scanner(file);
            _result = new ParserResult(_scanner.FileLines);
            TokenStack = _scanner._stack;
        }

        public ParserResult Parse()
        {
            ParseProgram();
            return _result;
        }

        // calls the functions to parse the program header and program body
        void ParseProgram()
        {
            ParseProgramHeader();
        }

        void ParseProgramHeader()
        {
            var programToken = TokenStack.Pop();
            if (programToken.Type != Type.KEYWORD && programToken.KeywordValue != Keyword.PROGRAM)
            {
                _result.AddErrorMessage(programToken, "Expected 'PROGRAM' keyword");
            }

            var identifierToken = TokenStack.Pop();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier");
            }

            var isToken = TokenStack.Pop();
            if (isToken.Type != Type.KEYWORD && isToken.KeywordValue != Keyword.IS)
            {
                _result.AddErrorMessage(isToken, "Expected 'IS' keyword");
            }
        }

        void ParseProgramBody()
        {
            var beginToken = TokenStack.Pop();
            while (beginToken.Type != Type.KEYWORD && beginToken.KeywordValue != Keyword.BEGIN)
            {
                TokenStack.Push(beginToken);
                ParseDeclaration();
                beginToken = TokenStack.Pop();
            }

            var endToken = TokenStack.Pop();
            while (endToken.Type != Type.KEYWORD && beginToken.KeywordValue != Keyword.END)
            {
                TokenStack.Push(endToken);
                ParseStatement();
                endToken = TokenStack.Pop();
            }

            var programToken = TokenStack.Pop();
            if (programToken.Type != Type.KEYWORD && programToken.KeywordValue != Keyword.PROGRAM)
            {
                _result.AddErrorMessage(programToken, "Expected 'Program' keyword");
            }
        }

        void ParseDeclaration()
        {
            var globalToken = TokenStack.Pop();
            if (globalToken.Type != Type.KEYWORD && globalToken.KeywordValue != Keyword.GLOBAL)
            {
                TokenStack.Push(globalToken);
            }

            var procedureToken = TokenStack.Pop();
            TokenStack.Push(procedureToken);
            if (procedureToken.Type != Type.KEYWORD && procedureToken.KeywordValue != Keyword.PROCEDURE)
            {
                ParseVariableDeclaration();
            }
            else
            {
                ParseProcedureDeclaration();
            }

            var semicolonToken = TokenStack.Pop();
            if (semicolonToken.Type != Type.SPECIAL && semicolonToken.SpecialValue != Special.SEMICOLON)
            {
                _result.AddWarningMessage(semicolonToken, "Missing semi-colon");
                TokenStack.Push(semicolonToken);
            }
        }

        void ParseProcedureDeclaration()
        {
            ParseProcedureHeader();
            ParseProcedureBody();
        }

        void ParseProcedureHeader()
        {
            var procedureToken = TokenStack.Pop();
            if (procedureToken.Type != Type.KEYWORD && procedureToken.KeywordValue != Keyword.PROCEDURE)
            {
                _result.AddErrorMessage(procedureToken, "WTF - this should be a 'Procedure' keyword");
            }

            var identifierToken = TokenStack.Pop();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier");
            }

            var leftParenToken = TokenStack.Pop();
            if (leftParenToken.Type != Type.SPECIAL && leftParenToken.SpecialValue != Special.LEFT_PAREN)
            {
                _result.AddErrorMessage(leftParenToken, "Expected open paren");
            }

            var rightParenToken = TokenStack.Pop();
            if (rightParenToken.Type != Type.SPECIAL && rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                TokenStack.Push(rightParenToken);
                ParseParameterList();
            }

            rightParenToken = TokenStack.Pop();
            if (rightParenToken.Type != Type.SPECIAL && rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                _result.AddErrorMessage(rightParenToken, "Expected right paren");
            }
        }

        void ParseParameterList()
        {
            ParseParameter();
            var commaToken = TokenStack.Pop();
            if (commaToken.Type == Type.SPECIAL && commaToken.SpecialValue == Special.COMMA)
            {
                ParseParameterList();
            }
        }

        void ParseParameter()
        {
            ParseVariableDeclaration();

            var accessToken = TokenStack.Pop();
            if (accessToken.Type != Type.KEYWORD && !(accessToken.KeywordValue == Keyword.IN || accessToken.KeywordValue == Keyword.OUT || accessToken.KeywordValue == Keyword.INOUT))
            {
                _result.AddErrorMessage(accessToken, "Expected access level");
            }
        }

        void ParseProcedureBody()
        {
            var beginToken = TokenStack.Pop();
            while (beginToken.Type != Type.KEYWORD && beginToken.KeywordValue != Keyword.BEGIN)
            {
                TokenStack.Push(beginToken);
                ParseDeclaration();
                beginToken = TokenStack.Pop();
            }

            var endToken = TokenStack.Pop();
            while (endToken.Type != Type.KEYWORD && beginToken.KeywordValue != Keyword.END)
            {
                TokenStack.Push(endToken);
                ParseStatement();
                endToken = TokenStack.Pop();
            }

            var programToken = TokenStack.Pop();
            if (programToken.Type != Type.KEYWORD && programToken.KeywordValue != Keyword.PROCEDURE)
            {
                _result.AddErrorMessage(programToken, "Expected 'Procedure' keyword");
            }
        }

        void ParseVariableDeclaration()
        {
            var typeMarkToken = TokenStack.Pop();
            if (typeMarkToken.Type != Type.KEYWORD && !(typeMarkToken.KeywordValue == Keyword.INTEGER || typeMarkToken.KeywordValue == Keyword.FLOAT || typeMarkToken.KeywordValue == Keyword.STRING || typeMarkToken.KeywordValue == Keyword.BOOL || typeMarkToken.KeywordValue == Keyword.CHAR))
            {
                _result.AddErrorMessage(typeMarkToken, "Expected type mark");
            }

            var identifierToken = TokenStack.Pop();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier");
            }

            var leftBracketToken = TokenStack.Pop();
            if (leftBracketToken.Type != Type.SPECIAL && leftBracketToken.SpecialValue != Special.LEFT_BRACKET)
            {
                TokenStack.Push(leftBracketToken);
                return;
            }

            var negativeToken = TokenStack.Pop();
            if (negativeToken.Type != Type.SPECIAL && negativeToken.SpecialValue != Special.NEGATIVE)
            {
                TokenStack.Push(negativeToken);
            }

            var lowerBoundToken = TokenStack.Pop();
            if (lowerBoundToken.Type != Type.INTEGER)
            {
                _result.AddErrorMessage(lowerBoundToken, "Expected integer");
            }

            var colonToken = TokenStack.Pop();
            if (colonToken.Type != Type.SPECIAL && colonToken.SpecialValue != Special.COLON)
            {
                _result.AddErrorMessage(colonToken, "Expected colon");
            }

            negativeToken = TokenStack.Pop();
            if (negativeToken.Type != Type.SPECIAL && negativeToken.SpecialValue != Special.NEGATIVE)
            {
                TokenStack.Push(negativeToken);
            }

            var upperBoundToken = TokenStack.Pop();
            if (upperBoundToken.Type != Type.INTEGER)
            {
                _result.AddErrorMessage(upperBoundToken, "Expected integer");
            }

            var rightBracketToken = TokenStack.Pop();
            if (rightBracketToken.Type != Type.SPECIAL && rightBracketToken.SpecialValue != Special.RIGHT_BRACKET)
            {
                _result.AddErrorMessage(rightBracketToken, "Expected right bracket token");
            }
        }

        void ParseStatement()
        {
            var token = TokenStack.Pop();
            if (token.Type == Type.KEYWORD && token.KeywordValue == Keyword.RETURN)
            {
                //return statement
                return;
            }
            else if (token.Type == Type.KEYWORD && token.KeywordValue == Keyword.IF)
            {
                TokenStack.Push(token);
                ParseIfStatement();
            }
            else if (token.Type == Type.KEYWORD && token.KeywordValue == Keyword.FOR)
            {
                TokenStack.Push(token);
                ParseLoopStatement();
            }
        }

        void ParseProcedureCall()
        {

        }

        void ParseAssignment()
        {

        }

        void ParseDestination()
        {

        }

        void ParseIfStatement()
        {

        }

        void ParseLoopStatement()
        {

        }

        void ParseExpression()
        {

        }

        void ParseArithOp()
        {

        }

        void ParseRelation()
        {

        }

        void ParseTerm()
        {

        }

        void ParseFactor()
        {

        }

        void ParseWord()
        {

        }

        void ParseName()
        {

        }

        void ParseArgumentList()
        {

        }
    }
}