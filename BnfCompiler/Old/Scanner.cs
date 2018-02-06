using System;
using System.Collections.Generic;
using System.IO;

namespace BnfCompiler
{
    public class Scanner3
    {
        readonly StreamReader _reader;

        readonly string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        readonly string numbers = "0123456789";
        readonly List<string> operators = new List<string> { "=", "==", "<", "<=", ">", ">=", "+", "-", "/", "*"};
        //List<Expression> _expressionList;

        int _currentRow;
        int _currentCol;

        //Expression _currentExpression;

        public Scanner3(string file)
        {
            FileStream fileStream = new FileStream(file, FileMode.Open);
            _reader = new StreamReader(fileStream);

            //_expressionList = new List<Expression>();

            _currentRow = 1;
            _currentCol = 1;

            //_currentExpression = new Expression();
        }

        //char ReadNextToken()
        //{
        //    var token = (char)_reader.Read();
        //    _currentExpression.ExpressionString += token.ToString();

        //    _currentExpression.Row = _currentExpression.Row ?? _currentRow;
        //    _currentExpression.Col = _currentExpression.Col ?? _currentCol;

        //    if (token == '\n')
        //    {
        //        _currentCol = 1;
        //        _currentRow += 1;
        //    }
        //    else
        //    {
        //        _currentCol += 1;
        //    }

        //    return token;
        //}

        //char PeekNextToken()
        //{
        //    return (char)_reader.Peek();
        //}

        //void EndExpression()
        //{
        //    _expressionList.Add(_currentExpression);
        //    _currentExpression = new Expression();
        //}

        //public void ScanFile()
        //{
        //    while (!_reader.EndOfStream)
        //    {
        //        var token = ReadNextToken();


        //        if (token == '/')
        //        {
        //            if (PeekNextToken() == '/')
        //            {
        //                _currentExpression.Type = ExpressionType.Comment;
        //                do
        //                {
        //                    ReadNextToken();
        //                }
        //                while (PeekNextToken() != '\n');
        //                EndExpression();
        //            }
        //            else if (PeekNextToken() == '*')
        //            {
        //                _currentExpression.Type = ExpressionType.Comment;
        //                ReadNextToken();
        //                var level = 1;
        //                do
        //                {
        //                    var currentToken = ReadNextToken();
        //                    if (currentToken == '/' && PeekNextToken() == '*')
        //                    {
        //                        ReadNextToken();
        //                        level += 1;
        //                    }
        //                    else if (currentToken == '*' && PeekNextToken() == '/')
        //                    {
        //                        ReadNextToken();
        //                        level -= 1;
        //                    }
        //                }
        //                while (level != 0);
        //                EndExpression();
        //            }
        //            else
        //            {
        //                _currentExpression.Type = ExpressionType.Operator;
        //            }

        //        }
        //        else if (letters.Contains(token.ToString()))
        //        {
        //            while (letters.Contains(PeekNextToken().ToString()) || numbers.Contains(PeekNextToken().ToString()) || PeekNextToken() == '_') 
        //            {
        //                ReadNextToken();    
        //            }
        //            _currentExpression.Type = ExpressionType.String;

        //            //TODO determine if expression is a reserved word

        //            EndExpression();
        //        }
        //        else if (token == '+') {
                    
        //        }
        //        else if (token == '-') {
                    
        //        }
        //        else if (token == '=')
        //        {
        //            if (PeekNextToken() == '=')
        //            {
        //                ReadNextToken();
        //                _currentExpression.Operator = OperationType.DoubleEquals;
        //                _currentExpression.Type = ExpressionType.Operator;
        //            }
        //            else 
        //            {
        //                _currentExpression.Operator = OperationType.Equals;
        //                _currentExpression.Type = ExpressionType.Operator;
        //            }
        //            EndExpression();
        //        }
        //        else if (token == '<')
        //        {
        //            //TODO handle <=
        //        }
        //        else if (token == '>')
        //        {
        //            //TODO handle >=
        //        }
        //        else if (token == '*')
        //        {

        //        }
        //        else if (token == '(')
        //        {

        //        }
        //        else if (token == ')')
        //        {

        //        }
        //        else if (token == '\n')
        //        {

        //        }
        //        else if (token == ' ')
        //        {

        //        }


        //    }

        //    DisplayExpressionList();
        //}

        //void DisplayExpressionList()
        //{
        //    for (int i = 0; i < _expressionList.Count; i++)
        //    {

        //        var expression = _expressionList[i].ExpressionString.Replace("\n", "");

        //        Console.Write("| ");
        //        Console.Write(expression);
        //        Console.SetCursorPosition(60, Console.CursorTop);
        //        Console.Write(" | ");
        //        Console.Write(GetExpressionType(_expressionList[i]));
        //        Console.SetCursorPosition(75, Console.CursorTop);
        //        Console.Write(" |");
        //        Console.WriteLine();
        //    }
        //}

        //string GetExpressionType(Expression expression)
        //{
        //    switch (expression.Type)
        //    {
        //        case ExpressionType.Boolean:
        //            return "Boolean";
        //        case ExpressionType.Char:
        //            return "Char";
        //        case ExpressionType.Comment:
        //            return "Comment";
        //        case ExpressionType.Float:
        //            return "Float";
        //        case ExpressionType.Integer:
        //            return "Integer";
        //        case ExpressionType.Operator:
        //            return "Operator";
        //        case ExpressionType.Reserved:
        //            return "Reserved";
        //        case ExpressionType.String:
        //            return "String";
        //        default:
        //            return "";
        //    }
        //}
    }
}
