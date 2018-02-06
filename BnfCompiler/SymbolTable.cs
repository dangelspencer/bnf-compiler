using System;
using System.Collections.Generic;

namespace BnfCompiler
{
    // TODO Add the rest of the symbols
    public enum SymbolType 
    {
        OpenParen,
        CloseParen,
        Plus,
        Minus,
        Divide,
        Multiply,
        Equals,
        DoubleEquals,
        Undefined
    }

    public class Symbol 
    {
        public string Value { get; set; }
        public SymbolType Type { get; set; }

        public Symbol(string value, SymbolType type) 
        {
            Value = value;
            Type = type;
        }
    }

    // TODO Add scope 
    public class SymbolTable
    {
        List<Symbol> _table;

        public SymbolTable()
        {
            _table = new List<Symbol>();
        }

        public void AddSymbolToTable(Symbol symbol) 
        {
            if (ContainsSymbol(symbol.Value)) 
            {
                throw new Exception("Symbol already exists");
            }

            _table.Add(symbol);
        }

        public bool ContainsSymbol (string value) 
        {
            foreach (var symbol in _table) 
            {
                if (symbol.Value == value) return true;
            }
            return false;
        }
    }
}
