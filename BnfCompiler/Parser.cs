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
        public List<string> ErrorMessages;
        private List<string> FileLines;
        private bool _debug;

        public ParserResult(List<string> fileLines, bool debug)
        {
            Messages = new List<string>();
            ErrorMessages = new List<string>();
            FileLines = fileLines;
            _debug = debug;
        }

        public void AddPlainMessage(Token token, string description)
        {
            AddMessage(description, token);
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
            AddMessageToErrorList(desciption, token);
        }

        public void AddErrorMessage(Token token, string message)
        {
            Success = false;
            var desciption = $"Line {token.LineIndex + 1}, Column {token.CharIndex + 1} - Error: {message}";
            AddMessage(desciption, token);
            AddMessageToErrorList(desciption, token);
        }

        public void AddPlainWarningMessage(string message)
        {
            Messages.Add(message);
        }

        public void AddPlainErrorMessage(string message)
        {
            Success = false;
            Messages.Add(message);
            ErrorMessages.Add(message);
        }

        private void AddMessage(string description, Token token)
        {
            if (_debug) 
            {
                Messages.Add(description);
                Console.WriteLine(description);
                Messages.Add(FileLines[token.LineIndex].Replace('\t', ' '));
                Messages.Add($"{GetFillerString(token.CharIndex, " ")}{GetFillerString(token.Value.Length, "^")}\n");
            }
        }

        private void AddMessageToErrorList(string description, Token token)
        {
            ErrorMessages.Add(description);
            ErrorMessages.Add(FileLines[token.LineIndex].Replace('\t', ' '));
            ErrorMessages.Add($"{GetFillerString(token.CharIndex, " ")}{GetFillerString(token.Value.Length, "^")}\n");
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
        public SymbolTable _table;

        private bool _debug;
        private Stack<Token> Stack;

        public Parser(string file, bool debug)
        {
            _scanner = new Scanner(file, debug);
            _result = new ParserResult(_scanner.FileLines, debug);
            _debug = debug;
            Stack = _scanner.Stack;

            for (var i = 0; i < _scanner.errors.Count; i++)
            {
                _result.AddErrorMessage(_scanner.errorTokens[i], _scanner.errors[i]);
            }
        }

        public ParserResult Parse()
        {
            try 
            {
                ParseProgram();
            } 
            catch (Exception ex)
            {
                _result.AddPlainErrorMessage("Error: " + ex.Message);
            }
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
                _table = new SymbolTable(identifierToken);
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
            var count = 0;
            while (endToken.KeywordValue != Keyword.END)
            {
                if (count == Stack.Count)
                {
                    var unknownToken = Stack.Pop();
                    _result.AddErrorMessage(unknownToken, "Unable to parse token, removing it from the stack and rechecking");
                }
                count = Stack.Count;
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
            var globalToken = Stack.Pop();
            var procedureToken = Stack.Peek();
            if (globalToken.KeywordValue != Keyword.GLOBAL)
            {
                procedureToken = globalToken;
            }
            Stack.Push(globalToken);
            

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
            var isGlobal = false;
            var globalToken = Stack.Peek();
            if (globalToken.KeywordValue == Keyword.GLOBAL)
            {
                ProcessToken("GLOBAL");
                isGlobal = true;
                if (_table._currentScope != 1)
                {
                    _result.AddErrorMessage(globalToken, "Global declaration cannot be made here");
                }
            }

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
                if (_table.HasSymbol(identifierToken.Value))
                {
                    // throw new symbol already defined error
                    _result.AddErrorMessage(identifierToken, "Procedure is already defined");
                }
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
            
            var variableTypes = new List<VariableType>();

            _table.EnterScope();
            var rightParenToken = Stack.Peek();
            if (rightParenToken.SpecialValue != Special.RIGHT_PAREN)
            {
                ParseParameterList(variableTypes);
            }

            _table.InsertProcedureSymbol(identifierToken, variableTypes, isGlobal); // so we have access to the current procedure on the new scope for recursion

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

        void ParseParameterList(List<VariableType> parameterTypes)
        {
            ParseParameter(parameterTypes);
            var commaToken = Stack.Peek();
            if (commaToken.SpecialValue == Special.COMMA)
            {
                ProcessToken(",");
                ParseParameterList(parameterTypes);
            }
        }

        void ParseParameter(List<VariableType> parameterTypes)
        {
            ParseVariableDeclaration(parameterTypes);

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
            var count = 0;
            while (endToken.KeywordValue != Keyword.END)
            {
                if (count == Stack.Count)
                {
                    var unknownToken = Stack.Pop();
                    _result.AddErrorMessage(unknownToken, "Unable to parse token, removing it from the stack and rechecking");
                }
                count = Stack.Count;
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

            _table.ExitScope();
        }

        void ParseVariableDeclaration(List<VariableType> parameterTypes = null)
        {
            var variableType = VariableType.DEFAULT;
            var isGlobal = false;
            var globalToken = Stack.Peek();
            if (globalToken.KeywordValue == Keyword.GLOBAL)
            {
                ProcessToken("GLOBAL");
                isGlobal = true;
                if (_table._currentScope != 1)
                {
                    _result.AddErrorMessage(globalToken, "Global declaration cannot be made here");
                }
            }

            var typeMarkToken = Stack.Peek();
            if (!(typeMarkToken.KeywordValue == Keyword.INTEGER || typeMarkToken.KeywordValue == Keyword.FLOAT || typeMarkToken.KeywordValue == Keyword.STRING || typeMarkToken.KeywordValue == Keyword.BOOL || typeMarkToken.KeywordValue == Keyword.CHAR))
            {
                _result.AddErrorMessage(typeMarkToken, "Expected type mark");
            }
            else
            {
                ProcessToken("INTEGER | FLOAT | STRING | CHAR | BOOL");
                switch (typeMarkToken.KeywordValue)
                {
                    case Keyword.INTEGER:
                        variableType = VariableType.INTEGER;
                        break;
                    case Keyword.FLOAT:
                        variableType = VariableType.FLOAT;
                        break;
                    case Keyword.STRING:
                        variableType = VariableType.STRING;
                        break;
                    case Keyword.CHAR:
                        variableType = VariableType.CHAR;
                        break;
                    case Keyword.BOOL:
                        variableType = VariableType.BOOL;
                        break;
                }
            }

            if (parameterTypes != null)
            {
                parameterTypes.Add(variableType);
            }

            var identifierToken = Stack.Peek();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier");
            }
            else
            {
                ProcessToken("(IDENTIFIER)");
            
                var isArray = false;
                var leftBracketToken = Stack.Peek();
                if (leftBracketToken.SpecialValue != Special.LEFT_BRACKET)
                {
                    _table.InsertVariableSymbol(identifierToken, variableType, isGlobal, null);
                    return;
                }
                else
                {
                    ProcessToken("[");
                    isArray = true;
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

                if (!_table.HasSymbol(identifierToken.Value))
                {   List<int> arrayBounds = null;
                    if (isArray) arrayBounds = new List<int>{lowerBoundToken.IntValue, upperBoundToken.IntValue};
                    _table.InsertVariableSymbol(identifierToken, variableType, isGlobal, arrayBounds);
                }
                else
                {
                    // throw new symbol already defined error
                    _result.AddErrorMessage(identifierToken, "Variable is already defined");
                }
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
                if (!_table.HasSymbol(identifierToken.Value))
                {
                    _result.AddErrorMessage(identifierToken, "Procedure not defined or not visible in either local or global scope");
                }
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

            var parameterList = _table.GetSymbol(identifierToken.Value).ParameterTypes;
            // TODO: pass in an array of types for the procedure so the expressions can be checked for the right type
            ParseArgumentList(parameterList, 0);

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
            var assignmentTargetType = ParseDestination();

            var equalsToken = Stack.Peek();
            if (equalsToken.SpecialValue != Special.EQUALS)
            {
                _result.AddErrorMessage(equalsToken, "Expected equals token");
            }
            else
            {
                ProcessToken(":=");
            }

            ParseExpression(assignmentTargetType, true);
        }

        VariableType ParseDestination()
        {
            var identifierToken = Stack.Peek();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier token");
            }
            else
            {
                ProcessToken("(IDENTIFIER)");
                if (!_table.HasSymbol(identifierToken.Value))
                {
                    _result.AddErrorMessage(identifierToken, "Variable not defined or not visible in either local or global scope");                    
                }
            }

            var leftBracketToken = Stack.Peek();
            if (leftBracketToken.SpecialValue == Special.LEFT_BRACKET)
            {
                ProcessToken("[");

                ParseExpression(VariableType.INTEGER, true);

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

            return _table.GetSymbol(identifierToken.Value).VariableType;
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

            ParseExpression(VariableType.BOOL, true);

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
            var count = 0;
            while (!(endElseToken.KeywordValue == Keyword.END || endElseToken.KeywordValue == Keyword.ELSE))
            {
                if (count == Stack.Count)
                {
                    var unknownToken = Stack.Pop();
                    _result.AddErrorMessage(unknownToken, "Unable to parse token, removing it from the stack and rechecking");
                }
                count = Stack.Count;
                ParseStatement();
                endElseToken = Stack.Peek();
            }

            count = 0;
            if (endElseToken.KeywordValue == Keyword.ELSE)
            {
                ProcessToken("ELSE");

                while (endElseToken.KeywordValue != Keyword.END)
                {
                    if (count == Stack.Count)
                    {
                        var unknownToken = Stack.Pop();
                        _result.AddErrorMessage(unknownToken, "Unable to parse token, removing it from the stack and rechecking");
                    }
                    count = Stack.Count;
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

            ParseExpression(VariableType.BOOL, true);

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
            var count = 0;
            while (endToken.KeywordValue != Keyword.END)
            {
                if (count == Stack.Count)
                {
                    var unknownToken = Stack.Pop();
                    _result.AddErrorMessage(unknownToken, "Unable to parse token, removing it from the stack and rechecking");
                }
                count = Stack.Count;
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

        Expression ParseExpression(VariableType? expectedType = null, bool topLevel = false)
        {
            var notOp = false;
            var negativeToken = Stack.Peek();
            if (negativeToken.KeywordValue == Keyword.NOT)
            {
                ProcessToken("NOT");
                notOp = true;
            }

            var exp = ParseArithOp();
            if (notOp)
            {
                var topExp = new Expression();
                topExp.Operator = Operator.NOT;
                topExp.Right = exp;
                exp = topExp;
            }

            if (!topLevel) 
            {
                return exp;
            }
            
            var list = new List<string>();
            var errorList = new Dictionary<Token, string>();
            var returnType = exp.WalkExpression(list, errorList);
            if (returnType != null) 
            {
                _result.AddPlainMessage(negativeToken, "Expression statement: " + String.Join(" ", list.ToArray()) + " -> " + Enum.GetName(typeof(VariableType), returnType.VariableType));
            }
            else
            {
                _result.AddErrorMessage(negativeToken, "Failed to determine result type of expression");
            }

            foreach (var entry in errorList)
            {
                _result.AddErrorMessage(entry.Key, entry.Value);
            }

            if (expectedType != null && returnType != null)
            {
                if (expectedType != returnType.VariableType)
                {
                    if (expectedType == VariableType.BOOL && returnType.VariableType != VariableType.INTEGER)
                    {
                        _result.AddErrorMessage(negativeToken, $"Unable to cast type {Enum.GetName(typeof(VariableType), returnType.VariableType)} to type {Enum.GetName(typeof(VariableType), expectedType)}");
                    }

                    if (expectedType == VariableType.INTEGER && returnType.VariableType != VariableType.BOOL)
                    {
                        _result.AddErrorMessage(negativeToken, $"Unable to cast type {Enum.GetName(typeof(VariableType), returnType.VariableType)} to type {Enum.GetName(typeof(VariableType), expectedType)}");
                    }

                    if (expectedType == VariableType.FLOAT && returnType.VariableType != VariableType.INTEGER)
                    {
                        _result.AddErrorMessage(negativeToken, $"Unable to cast type {Enum.GetName(typeof(VariableType), returnType.VariableType)} to type {Enum.GetName(typeof(VariableType), expectedType)}");
                    }
                }
            }

            return null;
        }

        Expression ParseArithOp()
        {
            var exp = ParseRelation();

            var andOrToken = Stack.Peek();
            if (!(andOrToken.SpecialValue == Special.AND || andOrToken.SpecialValue == Special.OR))
            {
                return exp;
            }
            ProcessToken("& | (|)");
            var newExp = new Expression();
            newExp.Left = exp;
            switch (andOrToken.SpecialValue) {
                case Special.AND:
                    newExp.Operator = Operator.AND;
                    break;
                case Special.OR:
                    newExp.Operator = Operator.OR;
                    break;
            }
            newExp.OperationType = OperationType.LOGICAL;
            newExp.Right = ParseArithOp();
            newExp.OperationToken = andOrToken;
            return newExp;
        }

        Expression ParseRelation()
        {
            var exp = ParseTerm();

            var plusNegToken = Stack.Peek();
            if (!(plusNegToken.SpecialValue == Special.PLUS || plusNegToken.SpecialValue == Special.NEGATIVE))
            {
                return exp;
            }
            ProcessToken("+ | -");
            var newExp = new Expression();
            newExp.Left = exp;
            switch (plusNegToken.SpecialValue) {
                case Special.PLUS:
                    newExp.Operator = Operator.PLUS;
                    break;
                case Special.NEGATIVE:
                    newExp.Operator = Operator.MINUS;
                    break;
            }
            newExp.Right = ParseRelation();
            newExp.OperationType = OperationType.MATH;
            newExp.OperationToken = plusNegToken;
            return newExp;
        }

        Expression ParseTerm()
        {
            var exp = ParseFactor();

            var relationToken = Stack.Peek();
            if (!(relationToken.SpecialValue == Special.LESS_THAN || relationToken.SpecialValue == Special.LESS_THAN_OR_EQUAL || relationToken.SpecialValue == Special.GREATER_THAN || relationToken.SpecialValue == Special.GREATER_THAN_OR_EQUAL || relationToken.SpecialValue == Special.DOUBLE_EQUALS || relationToken.SpecialValue == Special.NOT_EQUAL))
            {
                return exp;
            }
            ProcessToken("RELATION OPERATOR");
            var newExp = new Expression();
            newExp.Left = exp;
            switch (relationToken.SpecialValue) {
                case Special.LESS_THAN:
                    newExp.Operator = Operator.LESS_THAN;
                    break;
                case Special.LESS_THAN_OR_EQUAL:
                    newExp.Operator = Operator.LESS_THAN_OR_EQUAL;
                    break;
                case Special.GREATER_THAN:
                    newExp.Operator = Operator.GREATER_THAN;
                    break;
                case Special.GREATER_THAN_OR_EQUAL:
                    newExp.Operator = Operator.GREATER_THAN_OR_EQUAL;
                    break;
                case Special.DOUBLE_EQUALS:
                    newExp.Operator = Operator.DOUBLE_EQUAL;
                    break;
                case Special.NOT_EQUAL:
                    newExp.Operator = Operator.NOT_EQUAL;
                    break;
            }
            newExp.Right = ParseTerm();
            newExp.OperationType = OperationType.RELATION;
            newExp.OperationToken = relationToken;
            return newExp;
        }

        Expression ParseFactor()
        {
            var exp = ParseWord();

            var multDivToken = Stack.Peek();
            if (!(multDivToken.SpecialValue == Special.MULTIPLY || multDivToken.SpecialValue == Special.DIVIDE))
            {
                return exp;
            }

            ProcessToken("* | /");
            var newExp = new Expression();
            newExp.Left = exp;
            switch (multDivToken.SpecialValue) {
                case Special.MULTIPLY:
                    newExp.Operator = Operator.MULTIPLY;
                    break;
                case Special.DIVIDE:
                    newExp.Operator = Operator.DIVIDE;
                    break;
            }
            newExp.OperationType = OperationType.MATH;
            newExp.Right = ParseFactor();
            newExp.OperationToken = multDivToken;
            return newExp;
        }

        Expression ParseWord()
        {
            var token = Stack.Peek();
            if (token.SpecialValue == Special.LEFT_PAREN)
            {
                ProcessToken("(");

                var exp = ParseExpression();

                var rightParenToken = Stack.Peek();
                if (rightParenToken.SpecialValue != Special.RIGHT_PAREN)
                {
                    _result.AddErrorMessage(rightParenToken, "Expected right paren token");
                }
                else
                {
                    ProcessToken(")");
                }

                return exp;
            }
            else if (token.Type == Type.STRING)
            {
                ProcessToken("STRING");
                var value = new Value();
                value.StringValue = token.StringValue;
                value.VariableType = VariableType.STRING;
                value.token = token;
                var exp = new Expression();
                exp.Value = value;
                return exp;
            }
            else if (token.Type == Type.CHAR)
            {
                ProcessToken("CHAR");
                var value = new Value();
                value.CharValue = token.CharValue;
                value.VariableType = VariableType.CHAR;
                value.token = token;
                var exp = new Expression();
                exp.Value = value;
                return exp;
            }
            else if (token.Type == Type.BOOL)
            {
                ProcessToken("TRUE | FALSE");
                var value = new Value();
                value.BoolValue = token.BoolValue;
                value.VariableType = VariableType.BOOL;
                value.token = token;
                var exp = new Expression();
                exp.Value = value;
                return exp;
            }
            else if (token.Type == Type.FLOAT)
            {
                ProcessToken("FLOAT");
                var value = new Value();
                value.FloatValue = token.FloatValue;
                value.VariableType = VariableType.FLOAT;
                value.token = token;
                var exp = new Expression();
                exp.Value = value;
                return exp;
            }
            else if (token.Type == Type.INTEGER)
            {
                ProcessToken("INTEGER");
                var value = new Value();
                value.IntValue = token.IntValue;
                value.VariableType = VariableType.INTEGER;
                value.token = token;
                var exp = new Expression();
                exp.Value = value;
                return exp;
            }
            else if (token.Type == Type.IDENTIFIER)
            {
                var value = ParseName();
                var exp = new Expression();
                exp.Value = value;
                return exp;
            }
            else if (token.SpecialValue == Special.NEGATIVE)
            {
                ProcessToken("-");
                var nextToken = Stack.Peek();
                if (nextToken.Type == Type.FLOAT)
                {
                    ProcessToken("FLOAT");
                    var value = new Value();
                    value.FloatValue = -nextToken.FloatValue; //the value is set to negative b/c of the "-" token above  
                    value.VariableType = VariableType.FLOAT;
                    value.token = nextToken;
                    var exp = new Expression();
                    exp.Value = value;
                    return exp;
                }
                else if (nextToken.Type == Type.INTEGER )
                {
                    ProcessToken("INTEGER");
                    var value = new Value();
                    value.IntValue = -nextToken.IntValue;
                    value.VariableType = VariableType.INTEGER;
                    value.token = nextToken;
                    var exp = new Expression();
                    exp.Value = value;
                    return exp;
                }
                else
                {
                    var value = ParseName();
                    var exp = new Expression();
                    exp.Value = value;
                    return exp;
                }
            }
            else
            {
                _result.AddErrorMessage(token, "Unexpected token");
                return null;
            }
        }

        Value ParseName(VariableType? targetType = null)
        {
            var identifierToken = Stack.Peek();
            if (identifierToken.Type != Type.IDENTIFIER)
            {
                _result.AddErrorMessage(identifierToken, "Expected identifier token");
            }
            else
            {
                ProcessToken("IDENTIFIER");
                if (!_table.HasSymbol(identifierToken.Value))
                {
                    _result.AddErrorMessage(identifierToken, "Variable not defined or not visible in either local or global scope");
                }
            }

            var leftBracketToken = Stack.Peek();
            if (leftBracketToken.SpecialValue == Special.LEFT_BRACKET)
            {
                ProcessToken("[");

                ParseExpression(VariableType.INTEGER, true);

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

            var symbol = _table.GetSymbol(identifierToken.Value);
            var value = new Value();
            value.VariableValue = identifierToken.Value;
            value.VariableType = symbol.VariableType;
            value.token = identifierToken;
            return value;
        }

        void ParseArgumentList(List<VariableType> parameterList, int count)
        {
            var token = Stack.Peek();
            if (token.SpecialValue == Special.RIGHT_PAREN)
            {
                return;
            }

            // TODO get expression types here so they can be checked
            ParseExpression(parameterList[count], true);



            token = Stack.Peek();
            if (token.SpecialValue == Special.COMMA)
            {
                ProcessToken(",");
                ParseArgumentList(parameterList, count++);
            }
        }
    }
}