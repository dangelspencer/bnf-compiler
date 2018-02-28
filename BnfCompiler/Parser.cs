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

        public void AddParseMessage(Token token, string tokenString)
        {
            var description = $"Line {token.LineIndex + 1}, Column {token.CharIndex + 1} - Parsed '{tokenString}' token";
            AddMessage(description, token);
        }

        public void AddWarningMessage(Token token, string message)
        {
            var desciption = $"Line {token.LineIndex + 1}, Column {token.CharIndex + 1} - Warning: {message}";
            AddMessage(desciption, token);
        }

        public void AddErrorMessage(Token token, string message)
        {
            Success = false;
            var desciption = $"Line {token.LineIndex + 1}, Column {token.CharIndex + 1} - Error: {message}";
            AddMessage(desciption, token);
        }

        public void AddPlainWarningMessage(string message)
        {
            Messages.Add(message);
        }

        public void AddPlainErrorMessage(string message)
        {
            Messages.Add(message);
        }

        private void AddMessage(string description, Token token)
        {
            Messages.Add(description);
            Console.WriteLine(description);
            Messages.Add(FileLines[token.LineIndex].Replace('\t', ' '));
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

        void ProcessToken(string tokenString)
        {
            var token = Stack.Pop();
            _result.AddParseMessage(token, tokenString);
        }

        void ParseProgram()
        {
            ParseProgramHeader();
            ParseProgramBody();
        }

        void ParseProgramHeader()
        {
            var programToken = Stack.Peek();
            if (programToken.KeywordValue != Keyword.PROGRAM)
            {
                _result.AddErrorMessage(programToken, "Expected 'PROGRAM' keyword");
            }
            else
            {
                ProcessToken("PROGRAM");
            }

            var identifierToken = Stack.Peek();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier");
            }
            else 
            {
                ProcessToken("(IDENTIFIER)");
            }

            var isToken = Stack.Peek();
            if (isToken.KeywordValue != Keyword.IS)
            {
                _result.AddErrorMessage(isToken, "Expected 'IS' keyword");
            }
            else 
            {
                ProcessToken("IS");
            }
        }

        void ParseProgramBody()
        {
            var beginToken = Stack.Peek();
            while (beginToken.KeywordValue != Keyword.BEGIN)
            {
                _result.AddPlainErrorMessage("Attempting to parse declaration");
                ParseDeclaration();
                beginToken = Stack.Peek();
            }
            if (beginToken.Type != Type.KEYWORD && beginToken.KeywordValue != Keyword.BEGIN)
            {
                _result.AddErrorMessage(beginToken, "Missing Begin token");
            }
            else
            {
                ProcessToken("BEGIN");
            }

            var endToken = Stack.Peek();
            while (endToken.KeywordValue != Keyword.END)
            {
                ParseStatement();
                endToken = Stack.Peek();
            }

            ProcessToken("END");

            var programToken = Stack.Peek();
            if (programToken.KeywordValue != Keyword.PROGRAM)
            {
                _result.AddErrorMessage(programToken, "Expected 'Program' keyword");
            }
            else
            {
                ProcessToken("PROGRAM");
            }
            
            Token periodToken = null;
            Stack.TryPeek(out periodToken);
            if (periodToken == null || periodToken.SpecialValue != Special.PERIOD)
            {
                _result.AddPlainWarningMessage("Expected ending '.'");
            }
            else
            {
                ProcessToken(".");
            }
        }

        void ParseDeclaration()
        {
            _result.AddPlainErrorMessage("Parsing Declaration");
            var globalToken = Stack.Peek();
            if (globalToken.KeywordValue == Keyword.GLOBAL)
            {
                ProcessToken("GLOBAL");
            }

            var procedureToken = Stack.Peek();
            if (procedureToken.KeywordValue != Keyword.PROCEDURE)
            {
                ParseVariableDeclaration();
            }
            else
            {
                ParseProcedureDeclaration();
            }

            var semicolonToken = Stack.Peek();
            if (semicolonToken.SpecialValue != Special.SEMICOLON)
            {
                _result.AddWarningMessage(semicolonToken, "Missing semi-colon");
            }
            else
            {
                ProcessToken(";");
            }
        }

        void ParseProcedureDeclaration()
        {
            ParseProcedureHeader();
            ParseProcedureBody();
        }

        void ParseProcedureHeader()
        {
            var procedureToken = Stack.Peek();
            if (procedureToken.KeywordValue != Keyword.PROCEDURE)
            {
                _result.AddErrorMessage(procedureToken, "WTF - this should be a 'Procedure' keyword");
            }
            else
            {
                ProcessToken("PROCEDURE");
            }

            var identifierToken = Stack.Peek();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier");
            }
            else
            {
                ProcessToken("(IDENTIFIER)");
            }

            var leftParenToken = Stack.Peek();
            if (leftParenToken.SpecialValue != Special.LEFT_PAREN)
            {
                _result.AddErrorMessage(leftParenToken, "Expected open paren");
            }
            else
            {
                ProcessToken("(");
            }

            var rightParenToken = Stack.Peek();
            if (rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                ParseParameterList();
            }

            rightParenToken = Stack.Peek();
            if (rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                _result.AddErrorMessage(rightParenToken, "Expected right paren");
            }
            else
            {
                ProcessToken(")");
            }
        }

        void ParseParameterList()
        {
            ParseParameter();
            var commaToken = Stack.Peek();
            if (commaToken.SpecialValue == Special.COMMA)
            {
                ProcessToken(",");
                ParseParameterList();
            }
        }

        void ParseParameter()
        {
            ParseVariableDeclaration();

            var accessToken = Stack.Peek();
            if (!(accessToken.KeywordValue == Keyword.IN || accessToken.KeywordValue == Keyword.OUT || accessToken.KeywordValue == Keyword.INOUT))
            {
                _result.AddErrorMessage(accessToken, "Expected access level");
            }
            else
            {
                ProcessToken("IN | OUT | INOUT");
            }
        }

        void ParseProcedureBody()
        {
            var beginToken = Stack.Peek();
            while (beginToken.KeywordValue != Keyword.BEGIN)
            {
                ParseDeclaration();
                beginToken = Stack.Peek();
            }

            ProcessToken("BEGIN");

            var endToken = Stack.Peek();
            while (endToken.KeywordValue != Keyword.END)
            {
                ParseStatement();
                endToken = Stack.Peek();
            }

            ProcessToken("END");

            var procedureToken = Stack.Peek();
            if (procedureToken.KeywordValue != Keyword.PROCEDURE)
            {
                _result.AddErrorMessage(procedureToken, "Expected 'Procedure' keyword");
            }
            else
            {
                ProcessToken("PROCEDURE");
            }
        }

        void ParseVariableDeclaration()
        {
            var typeMarkToken = Stack.Peek();
            if (!(typeMarkToken.KeywordValue == Keyword.INTEGER || typeMarkToken.KeywordValue == Keyword.FLOAT || typeMarkToken.KeywordValue == Keyword.STRING || typeMarkToken.KeywordValue == Keyword.BOOL || typeMarkToken.KeywordValue == Keyword.CHAR))
            {
                _result.AddErrorMessage(typeMarkToken, "Expected type mark");
            }
            else
            {
                ProcessToken("INTEGER | FLOAT | STRING | CHAR | BOOL");
            }

            var identifierToken = Stack.Peek();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier");
            }
            else
            {
                ProcessToken("(IDENTIFIER)");
            }

            var leftBracketToken = Stack.Peek();
            if (leftBracketToken.SpecialValue != Special.LEFT_BRACKET)
            {
                return;
            }
            else
            {
                ProcessToken("[");
            }

            var negativeToken = Stack.Peek();
            if (negativeToken.SpecialValue == Special.NEGATIVE)
            {
                ProcessToken("-");
            }

            var lowerBoundToken = Stack.Peek();
            if (lowerBoundToken.Type != Type.FLOAT && lowerBoundToken.Type != Type.INTEGER)
            {
                _result.AddErrorMessage(lowerBoundToken, "Expected integer");
            }
            else
            {
                ProcessToken("Array Lower Bound");
            }

            var colonToken = Stack.Peek();
            if (colonToken.SpecialValue != Special.COLON)
            {
                _result.AddErrorMessage(colonToken, "Expected colon");
            }
            else
            {
                ProcessToken(":");
            }

            negativeToken = Stack.Peek();
            if (negativeToken.SpecialValue == Special.NEGATIVE)
            {
                ProcessToken("-");
            }

            var upperBoundToken = Stack.Peek();
            if (upperBoundToken.Type != Type.INTEGER && upperBoundToken.Type != Type.FLOAT)
            {
                _result.AddErrorMessage(upperBoundToken, "Expected integer");
            }
            else
            {
                ProcessToken("Array Upper Bound");
            }

            var rightBracketToken = Stack.Peek();
            if (rightBracketToken.SpecialValue != Special.RIGHT_BRACKET)
            {
                _result.AddErrorMessage(rightBracketToken, "Expected right bracket token");
            }
            else
            {
                ProcessToken("]");
            }
        }

        void ParseStatement()
        {
            var token = Stack.Peek();
            if (token.KeywordValue == Keyword.RETURN)
            {
                ProcessToken("RETURN");
            }
            else if (token.KeywordValue == Keyword.IF)
            {
                ParseIfStatement();
            }
            else if (token.KeywordValue == Keyword.FOR)
            {
                ParseLoopStatement();
            }
            else if (token.Type == Type.IDENTIFIER) 
            {
                token = Stack.Pop();
                var nextToken = Stack.Peek();
                if (nextToken.SpecialValue == Special.LEFT_PAREN)
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

            var semicolonToken = Stack.Peek();
            if (semicolonToken.SpecialValue != Special.SEMICOLON)
            {
                _result.AddWarningMessage(semicolonToken, "Missing semi colon");
            }
            else
            {
                ProcessToken(";");
            }
        }

        void ParseProcedureCall()
        {
            var identifierToken = Stack.Peek();
            if (identifierToken.Type != Type.IDENTIFIER) 
            {
                _result.AddErrorMessage(identifierToken, "Expected Identifier token");
            }
            else
            {
                ProcessToken("IDENTIFIER");
            }

            var leftParenToken = Stack.Peek();
            if (leftParenToken.SpecialValue != Special.LEFT_PAREN)
            {
                _result.AddErrorMessage(leftParenToken, "Expected left paren token");
            }
            else
            {
                ProcessToken("(");
            }

            ParseArgumentList();

            var rightParenToken = Stack.Peek();
            if (rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                _result.AddErrorMessage(rightParenToken, "Expected right paren token");
            }
            else
            {
                ProcessToken(")");
            }
        }

        void ParseAssignment()
        {
            ParseDestination();

            var equalsToken = Stack.Peek();
            if (equalsToken.SpecialValue != Special.EQUALS)
            {
                _result.AddErrorMessage(equalsToken, "Expected equals token");
            }
            else
            {
                ProcessToken(":=");
            }

            ParseExpression();
        }

        void ParseDestination()
        {
            var identifierToken = Stack.Peek();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier token");
            }
            else
            {
                ProcessToken("(IDENTIFIER)");
            }

            var leftBracketToken = Stack.Peek();
            if (leftBracketToken.SpecialValue != Special.LEFT_BRACKET)
            {
                return;
            }
            else
            {
                ProcessToken("[");
            }

            ParseExpression();

            var rightBracketToken = Stack.Peek();
            if (rightBracketToken.SpecialValue != Special.RIGHT_BRACKET)
            {
                _result.AddErrorMessage(rightBracketToken, "Expected right bracket token");
            }
            else
            {
                ProcessToken("]");
            }
        }

        void ParseIfStatement()
        {
            var ifToken = Stack.Peek();
            if (ifToken.KeywordValue != Keyword.IF)
            {
                _result.AddErrorMessage(ifToken, "WTF? this should be an if token");
            }
            else
            {
                ProcessToken("IF");
            }

            var leftParenToken = Stack.Peek();
            if (leftParenToken.SpecialValue != Special.LEFT_PAREN)
            {
                _result.AddErrorMessage(leftParenToken, "Expected left paren token");
            }
            else
            {
                ProcessToken("(");
            }

            ParseExpression();

            var rightParenToken = Stack.Peek();
            if (rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                _result.AddErrorMessage(rightParenToken, "Expected right paren token");
            }
            else
            {
                ProcessToken(")");
            }

            var thenToken = Stack.Peek();
            if (thenToken.KeywordValue != Keyword.THEN)
            {
                _result.AddErrorMessage(thenToken, "Expected 'then' token");
            }
            else
            {
                ProcessToken("THEN");
            }

            var endElseToken = Stack.Peek();
            while (!(endElseToken.KeywordValue == Keyword.END || endElseToken.KeywordValue == Keyword.ELSE))
            {
                ParseStatement();
                endElseToken = Stack.Peek();
            }

            if (endElseToken.KeywordValue == Keyword.ELSE)
            {
                ProcessToken("ELSE");

                while (endElseToken.KeywordValue != Keyword.END)
                {
                    ParseStatement();
                    endElseToken = Stack.Peek();
                }
            }

            ProcessToken("END");
            
            ifToken = Stack.Peek();
            if (ifToken.KeywordValue != Keyword.IF)
            {
                _result.AddErrorMessage(ifToken, "Missing end if token");
            }
            else
            {
                ProcessToken("IF");
            }
        }

        void ParseLoopStatement()
        {
            var forToken = Stack.Peek();
            if (forToken.KeywordValue != Keyword.FOR)
            {
                _result.AddErrorMessage(forToken, "WTF? This should be a for token");
            }
            else
            {
                ProcessToken("FOR");
            }

            var leftParenToken = Stack.Peek();
            if (leftParenToken.SpecialValue != Special.LEFT_PAREN)
            {
                _result.AddErrorMessage(leftParenToken, "Expected left paren token");
            }
            else
            {
                ProcessToken("(");
            }

            ParseAssignment();

            var semicolonToken = Stack.Peek();
            if (semicolonToken.SpecialValue != Special.SEMICOLON)
            {
                _result.AddErrorMessage(semicolonToken, "Missing semi colon");
            }
            else
            {
                ProcessToken(";");
            }

            ParseExpression();

            var rightParenToken = Stack.Peek();
            if (rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                _result.AddErrorMessage(rightParenToken, "Expected right paren token");
            }
            else
            {
                ProcessToken(")");
            }

            var endToken = Stack.Peek();
            while (endToken.KeywordValue != Keyword.END)
            {
                ParseStatement();
                endToken = Stack.Peek();
            }

            ProcessToken("END");

            forToken = Stack.Peek();
            if (forToken.KeywordValue != Keyword.FOR)
            {
                _result.AddErrorMessage(forToken, "Missing ending for token");
            } 
            else
            {
                ProcessToken("FOR");
            }
        }

        void ParseExpression()
        {
            var negativeToken = Stack.Peek();
            if (negativeToken.KeywordValue == Keyword.NOT)
            {
                ProcessToken("NOT");
            }

            ParseArithOp();
        }

        void ParseArithOp()
        {
            ParseRelation();

            var andOrToken = Stack.Peek();
            if (!(andOrToken.SpecialValue == Special.AND || andOrToken.SpecialValue == Special.OR))
            {
                return;
            }
            else
            {
                ProcessToken("& | (|)");
            }

            ParseArithOp();
        }

        void ParseRelation()
        {
            ParseTerm();

            var plusNegToken = Stack.Peek();
            if (!(plusNegToken.SpecialValue == Special.PLUS || plusNegToken.SpecialValue == Special.NEGATIVE))
            {
                return;
            }
            else
            {
                ProcessToken("+ | -");
            }

            ParseRelation();
        }

        void ParseTerm()
        {
            ParseFactor();

            var relationToken = Stack.Peek();
            if (!(relationToken.SpecialValue == Special.LESS_THAN || relationToken.SpecialValue == Special.LESS_THAN_OR_EQUAL || relationToken.SpecialValue == Special.GREATER_THAN || relationToken.SpecialValue == Special.GREATER_THAN_OR_EQUAL || relationToken.SpecialValue == Special.DOUBLE_EQUALS || relationToken.SpecialValue == Special.NOT_EQUAL))
            {
                return;
            }
            else
            {
                ProcessToken("RELATION OPERATOR");
            }

            ParseTerm();
        }

        void ParseFactor()
        {
            ParseWord();

            var multDivToken = Stack.Peek();
            if (!(multDivToken.SpecialValue == Special.MULTIPLY || multDivToken.SpecialValue == Special.DIVIDE))
            {
                return;
            }
            else
            {
                ProcessToken("* | /");
            }

            ParseFactor();
        }

        void ParseWord()
        {
            var token = Stack.Peek();
            if (token.SpecialValue == Special.LEFT_PAREN)
            {
                ProcessToken("(");
                
                ParseExpression();

                var rightParenToken = Stack.Peek();
                if (rightParenToken.SpecialValue != Special.RIGHT_PAREN)
                {
                    _result.AddErrorMessage(rightParenToken, "Expected right paren token");
                }
                else
                {
                    ProcessToken(")");
                }
            }
            else if (token.Type == Type.STRING)
            {
                ProcessToken("STRING");
            }
            else if (token.Type == Type.CHAR)
            {
                ProcessToken("CHAR");
            }
            else if (token.Type == Type.BOOL)
            {
                ProcessToken("TRUE | FALSE");
            }
            else if (token.Type == Type.FLOAT || token.Type == Type.INTEGER)
            {
                ProcessToken("INTEGER | FLOAT");
            }
            else if (token.Type == Type.IDENTIFIER)
            {
                ParseName();
            }
            else if (token.SpecialValue == Special.NEGATIVE)
            {
                ProcessToken("-");
                var nextToken = Stack.Peek();
                if (nextToken.Type == Type.INTEGER || nextToken.Type == Type.FLOAT)
                {
                    // do nothing
                    ProcessToken("INTEGER | FLOAT");
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
            var identifierToken = Stack.Peek();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier token");
            }
            else
            {
                ProcessToken("IDENTIFIER");
            }

            var leftBracketToken = Stack.Peek();
            if (leftBracketToken.SpecialValue != Special.LEFT_BRACKET)
            {
                return;
            }
            else
            {
                ProcessToken("[");
            }

            ParseExpression();

            var rightBracketToken = Stack.Peek();
            if (rightBracketToken.SpecialValue != Special.RIGHT_BRACKET)
            {
                _result.AddErrorMessage(rightBracketToken, "Expected right paren token");
            }
            else
            {
                ProcessToken("]");
            }
        }

        void ParseArgumentList()
        {
            var token = Stack.Peek();
            if (token.SpecialValue == Special.RIGHT_PAREN)
            {
                return;
            }

            ParseExpression();

            token = Stack.Peek();
            if (token.SpecialValue == Special.COMMA)
            {
                ProcessToken(",");
                ParseArgumentList();
            }
        }
    }
}