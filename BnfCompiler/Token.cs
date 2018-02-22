using System;
namespace BnfCompiler
{
    public enum Type
    {
        IDENTIFIER,
        INTEGER,
        FLOAT,
        STRING,
        CHAR,
        KEYWORD,
        SPECIAL,
        UNKNOWN
    }

    public enum Keyword
    {
        PROGRAM,
        IS,
        BEGIN,
        END,
        GLOBAL,
        PROCEDURE,
        IN,
        OUT,
        INOUT,
        INTEGER,
        FLOAT,
        STRING,
        BOOL,
        CHAR,
        TRUE,
        FALSE,
        IF,
        THEN,
        ELSE,
        FOR,
        RETURN,
        NOT
    }

    public enum Special
    {
        SEMICOLON,
        QUOTE,
        SINGLE_QUOTE,
        LEFT_BRACKET,
        RIGHT_BRACKET,
        PERIOD,
        LEFT_PAREN,
        RIGHT_PAREN,
        COMMA,
        COLON,
        EQUALS,
        DOUBLE_EQUALS,
        AND,
        OR,
        LESS_THAN,
        LESS_THAN_OR_EQUAL,
        GREATER_THAN,
        GREATER_THAN_OR_EQUAL,
        MULTIPLY,
        DIVIDE,
        NEGATIVE,
        PLUS,
        EXCLAMATION,
        NOT_EQUAL
    }

    public class Token
    {
        public Token(string value, Type type, int lineIndex, int charIndex)
        {
            Value = value;
            Type = type;
            LineIndex = lineIndex;
            CharIndex = charIndex - (Value.Length - 1); //since the char index is the index of the last character in the token and we want the index of the first character

            if (Type == Type.IDENTIFIER || Type == Type.SPECIAL || Type == Type.UNKNOWN)
            {
                SetKeywordValue();
                SetSpecialValue();
            }
        }

        public string Value;
        public Type Type;
        public int ScopeLevel;
        public int LineIndex;
        public int CharIndex;
        public bool BoolValue;


        public Keyword KeywordValue;
        public Special SpecialValue;

        public int IntValue;
        public float FloatValue;
        public string StringValue;
        public char CharValue;



        private void SetKeywordValue()
        {
            var oldType = Type;
            Type = Type.KEYWORD;
            switch (Value.ToUpper())
            {
                case "PROGRAM":
                    KeywordValue = Keyword.PROGRAM;
                    break;
                case "IS":
                    KeywordValue = Keyword.IS;
                    break;
                case "BEGIN":
                    KeywordValue = Keyword.BEGIN;
                    break;
                case "END":
                    KeywordValue = Keyword.END;
                    break;
                case "GLOBAL":
                    KeywordValue = Keyword.GLOBAL;
                    break;
                case "PROCEDURE":
                    KeywordValue = Keyword.PROCEDURE;
                    break;
                case "IN":
                    KeywordValue = Keyword.IN;
                    break;
                case "OUT":
                    KeywordValue = Keyword.OUT;
                    break;
                case "INOUT":
                    KeywordValue = Keyword.INOUT;
                    break;
                case "INTEGER":
                    KeywordValue = Keyword.INTEGER;
                    break;
                case "FLOAT":
                    KeywordValue = Keyword.FLOAT;
                    break;
                case "STRING":
                    KeywordValue = Keyword.STRING;
                    break;
                case "BOOL":
                    KeywordValue = Keyword.BOOL;
                    break;
                case "CHAR":
                    KeywordValue = Keyword.CHAR;
                    break;
                case "TRUE":
                    BoolValue = true;
                    break;
                case "FALSE":
                    BoolValue = false;
                    break;
                case "IF":
                    KeywordValue = Keyword.IF;
                    break;
                case "THEN":
                    KeywordValue = Keyword.THEN;
                    break;
                case "ELSE":
                    KeywordValue = Keyword.ELSE;
                    break;
                case "FOR":
                    KeywordValue = Keyword.FOR;
                    break;
                case "RETURN":
                    KeywordValue = Keyword.RETURN;
                    break;
                case "NOT":
                    KeywordValue = Keyword.NOT;
                    break;
                default:
                    Type = oldType;
                    break;
            }
        }

        private void SetSpecialValue()
        {
            var oldType = Type;
            Type = Type.SPECIAL;
            switch (Value)
            {
                case ";":
                    SpecialValue = Special.SEMICOLON;
                    break;
                case "\"":
                    SpecialValue = Special.QUOTE;
                    break;
                case "'":
                    SpecialValue = Special.SINGLE_QUOTE;
                    break;
                case "[":
                    SpecialValue = Special.LEFT_BRACKET;
                    break;
                case "]":
                    SpecialValue = Special.RIGHT_BRACKET;
                    break;
                case ".":
                    SpecialValue = Special.PERIOD;
                    break;
                case "(":
                    SpecialValue = Special.LEFT_PAREN;
                    break;
                case ")":
                    SpecialValue = Special.RIGHT_PAREN;
                    break;
                case ",":
                    SpecialValue = Special.COMMA;
                    break;
                case ":":
                    SpecialValue = Special.COLON;
                    break;
                case ":=":
                    SpecialValue = Special.EQUALS;
                    break;
                case "==":
                    SpecialValue = Special.DOUBLE_EQUALS;
                    break;
                case "&":
                    SpecialValue = Special.AND;
                    break;
                case "|":
                    SpecialValue = Special.OR;
                    break;
                case "<":
                    SpecialValue = Special.LESS_THAN;
                    break;
                case "<=":
                    SpecialValue = Special.LESS_THAN_OR_EQUAL;
                    break;
                case ">":
                    SpecialValue = Special.GREATER_THAN;
                    break;
                case ">=":
                    SpecialValue = Special.GREATER_THAN_OR_EQUAL;
                    break;
                case "*":
                    SpecialValue = Special.MULTIPLY;
                    break;
                case "/":
                    SpecialValue = Special.DIVIDE;
                    break;
                case "-":
                    SpecialValue = Special.NEGATIVE;
                    break;
                case "+":
                    SpecialValue = Special.PLUS;
                    break;
                case "!":
                    SpecialValue = Special.EXCLAMATION;
                    break;
                case "!=":
                    SpecialValue = Special.NOT_EQUAL;
                    break;
                default:
                Type = oldType;
                    break;
            }
        }
    }
}
