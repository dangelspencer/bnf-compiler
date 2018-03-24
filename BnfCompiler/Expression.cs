using System;
using System.Collections.Generic;
using System.Linq;

namespace BnfCompiler
{
    public enum OperationType 
    {
        RELATION,
        LOGICAL,
        MATH
    }
    public enum Operator 
    {
        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE,
        AND,
        OR,
        LESS_THAN,
        LESS_THAN_OR_EQUAL,
        GREATER_THAN,
        GREATER_THAN_OR_EQUAL,
        DOUBLE_EQUAL,
        NOT_EQUAL,
        NOT
    }

    public class Expression {
        public Expression Left;
        public Expression Right;
        public Operator Operator;
        public Token OperationToken;
        public OperationType? OperationType;
        public Value Value;

        public Value WalkExpression(List<string> list, Dictionary<Token, string> errorList) 
        {
            if (Value != null)
            {
                list.Add(Value.GetStringRepresentation());
                return Value ?? null;
            }

            Value leftVal = null;
            Value rightVal = null;

            if (Left != null) 
            {
                list.Add("(");
                leftVal = Left.WalkExpression(list, errorList);
            }

            list.Add(Enum.GetName(typeof(Operator), Operator));

            if (Right != null)
            {
                rightVal = Right.WalkExpression(list, errorList);
                list.Add(")");
            }

            return CompareTypes(Operator, leftVal, rightVal, errorList);
        }

        public Value CompareTypes(Operator op, Value val1, Value val2, Dictionary<Token, string> errorList)
        {
            if ((val1 == null && op != Operator.NOT) || val2 == null)
            {
                return null;
            }

            if (op == Operator.PLUS || op == Operator.MINUS || op == Operator.MULTIPLY || op == Operator.DIVIDE)
            {
                // arithmetic operations
                if (val1.VariableType != VariableType.FLOAT && val1.VariableType != VariableType.INTEGER)
                {
                    errorList.Add(val1.token, $"Cannot perform arithmetic operation on type {Enum.GetName(typeof(VariableType), val1.VariableType)}");
                    return null;
                } 
                else if (val2.VariableType != VariableType.FLOAT && val2.VariableType != VariableType.INTEGER)
                {
                    errorList.Add(val2.token, $"Cannot perform arithmetic operation on type {Enum.GetName(typeof(VariableType), val2.VariableType)}");
                    return null;
                }
                else if (val1.VariableType == val2.VariableType) 
                {
                    return val1;
                }
                else 
                {
                    var val = new Value();
                    val.VariableType = VariableType.FLOAT;
                    return val;
                }
            }
            else if (op == Operator.AND || op == Operator.OR)
            {
                // logical operations
                if (val1.VariableType != VariableType.INTEGER)
                {
                    errorList.Add(val1.token, $"Logical AND and OR operations can only be performed on integers. Recieved: {Enum.GetName(typeof(VariableType), val1.VariableType)}");
                    return null;
                }

                if (val2.VariableType != VariableType.INTEGER)
                {
                    errorList.Add(val2.token, $"Logical AND and OR operations can only be performed on integers. Recieved: {Enum.GetName(typeof(VariableType), val2.VariableType)}");
                    return null;
                }

                var val = new Value();
                val.VariableType = VariableType.BOOL;
                return val;
            }
            else if (op == Operator.NOT)
            {
                if (val2.VariableType != VariableType.INTEGER)
                {
                    errorList.Add(val2.token, $"Logical NOT operation can only be performed on integers. Recieved: {Enum.GetName(typeof(VariableType), val2.VariableType)}");
                    return null;
                }

                var val = new Value();
                val.VariableType = VariableType.BOOL;
                return val;
            }
            else 
            {
                // relational operations
                if (val1.VariableType != val2.VariableType)
                {
                    if (!((val1.VariableType == VariableType.BOOL || val1.VariableType == VariableType.INTEGER) && (val2.VariableType == VariableType.BOOL || val2.VariableType == VariableType.INTEGER)))
                    {
                        errorList.Add(val1.token, $"Cannot use relational operator on types {Enum.GetName(typeof(VariableType), val1.VariableType)} and {Enum.GetName(typeof(VariableType), val2.VariableType)}");
                        return null;
                    }

                    if (!((val1.VariableType == VariableType.FLOAT || val1.VariableType == VariableType.INTEGER) && (val2.VariableType == VariableType.FLOAT || val2.VariableType == VariableType.INTEGER)))
                    {
                        errorList.Add(val1.token, $"Cannot use relational operator on types {Enum.GetName(typeof(VariableType), val1.VariableType)} and {Enum.GetName(typeof(VariableType), val2.VariableType)}");
                        return null;
                    }
                }

                var val = new Value();
                val.VariableType = VariableType.BOOL;
                return val;
            }
        }
    }

    public class Value {
        public int? IntValue;
        public float? FloatValue;
        public string StringValue;
        public char? CharValue;
        public bool? BoolValue;
        public string VariableValue;
        public bool IsNegative;
        public Token token;

        public VariableType VariableType;

        public string GetStringRepresentation() {
            if (IntValue != null)
            {
                return token.IntValue.ToString();
            }
            if (FloatValue != null) 
            {
                return token.FloatValue.ToString();
            }
            if (StringValue != null)
            {
                return StringValue;
            }
            if (CharValue != null)
            {
                return CharValue.ToString();
            }
            if (BoolValue != null)
            {
                if ((bool)BoolValue) return "true";
                return "false";
            }
            if (VariableValue != null)
            {
                return VariableValue;
            }

            return "...unknown...";
        }
    }
}