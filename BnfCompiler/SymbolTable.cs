using System;
using System.Collections.Generic;
using System.Linq;

namespace BnfCompiler
{
    public enum SymbolType 
    {
        VARIABLE,
        PROCEDURE,
        PROGRAM
    }

    public enum VariableType {
        INTEGER,
        FLOAT,
        STRING,
        CHAR,
        BOOL,
        DEFAULT
    }

    public class Symbol 
    {
        public SymbolType Type;
        public Token Token;
        public int Scope;
        public VariableType VariableType;
        public List<VariableType> ParameterTypes;
        public List<int> ArrayBounds;

        public Symbol() { }
    }

    public class SymbolTable
    {
        private List<Symbol> _table;
        public int _currentScope {
            get { return scopeList.Peek(); }
        }
        private int _scopeCount;
        private Stack<int> scopeList;

        public SymbolTable(Token programNameToken) 
        {
            _table = new List<Symbol>();
            _scopeCount = 0;
            scopeList = new Stack<int>();
            scopeList.Push(0);
            InsertProgramSymbol(programNameToken);

            //Insert I/O functions into global scope
            InsertProcedureSymbol(new Token("GETBOOL", Type.IDENTIFIER, 0, 0), new List<VariableType> {VariableType.BOOL}, true);
            InsertProcedureSymbol(new Token("GETINTEGER", Type.IDENTIFIER, 0, 0), new List<VariableType> {VariableType.INTEGER}, true);
            InsertProcedureSymbol(new Token("GETFLOAT", Type.IDENTIFIER, 0, 0), new List<VariableType> {VariableType.FLOAT}, true);
            InsertProcedureSymbol(new Token("GETSTRING", Type.IDENTIFIER, 0, 0), new List<VariableType> {VariableType.STRING}, true);
            InsertProcedureSymbol(new Token("GETCHAR", Type.IDENTIFIER, 0, 0), new List<VariableType> {VariableType.CHAR}, true);
            InsertProcedureSymbol(new Token("PUTBOOL", Type.IDENTIFIER, 0, 0), new List<VariableType> {VariableType.BOOL}, true);
            InsertProcedureSymbol(new Token("PUTINTEGER", Type.IDENTIFIER, 0, 0), new List<VariableType> {VariableType.INTEGER}, true);
            InsertProcedureSymbol(new Token("PUTFLOAT", Type.IDENTIFIER, 0, 0), new List<VariableType> {VariableType.FLOAT}, true);
            InsertProcedureSymbol(new Token("PUTSTRING", Type.IDENTIFIER, 0, 0), new List<VariableType> {VariableType.STRING}, true);
            InsertProcedureSymbol(new Token("PUTCHAR", Type.IDENTIFIER, 0, 0), new List<VariableType> {VariableType.CHAR}, true);
        }

        public void EnterScope(int scope = -1) // int? allows us to get a new scope by passing no parameters and still enter a previously defined scope if we want to
        {
            scopeList.Push(_scopeCount += 1);
        }

        public void ExitScope()
        {
            scopeList.Pop();
        }

        public void InsertProgramSymbol(Token token)
        {
            var symbol = new Symbol();
            symbol.Token = token;
            symbol.Type = SymbolType.PROGRAM;
            symbol.Scope = _currentScope;

            _table.Add(symbol);
            EnterScope();
        }

        public void InsertProcedureSymbol(Token token, List<VariableType> parameterTypes, bool isGlobal)
        {
            var symbol = new Symbol();
            symbol.Token = token;
            symbol.Type = SymbolType.PROCEDURE;
            symbol.Scope = _currentScope;
            symbol.ParameterTypes = parameterTypes;
            
            if (isGlobal)
            {
                symbol.Scope = 0;
                _table.Add(symbol);
            }
            else 
            {
                _table.Add(symbol);
            
                //also add symbol to scope above (if it's not the top level)
                if (scopeList.Count > 2)
                {
                    var temp = scopeList.Pop();
                    symbol = new Symbol();
                    symbol.Token = token;
                    symbol.Type = SymbolType.PROCEDURE;
                    symbol.Scope = _currentScope;
                    symbol.ParameterTypes = parameterTypes;
                    _table.Add(symbol);
                    scopeList.Push(temp);
                }   
            }
        }

        public void InsertVariableSymbol(Token token, VariableType type, bool isGlobal, List<int> arrayBounds = null)
        {
            
            var symbol = new Symbol();
            symbol.Token = token;
            symbol.Type = SymbolType.VARIABLE;
            if (isGlobal) {
                symbol.Scope = 0;
            } else {
                symbol.Scope = _currentScope;
            }
            symbol.VariableType = type;
            symbol.ArrayBounds = arrayBounds;

            _table.Add(symbol);
        }

        public bool HasSymbol(string value)
        {
            var symbol = _table.Where(x => (x.Scope == _currentScope || x.Scope == 0) && x.Token.Value == value).FirstOrDefault();
            if (symbol != null) 
            {
                return true;
            }
            return false;
        }

        public Symbol GetSymbol(string value)
        {
            return _table.Where(x => (x.Scope == _currentScope || x.Scope == 0) && x.Token.Value == value).FirstOrDefault();
        }

        public void PrintSymbols() 
        {
            var symbols = _table.OrderBy(x => x.Scope).ThenBy(x => x.Token.Value).ToList();

            foreach (var symbol in symbols)
            {
                Console.WriteLine($"Symbol: {symbol.Token.Value}\nScope: {symbol.Scope}\nType: {Enum.GetName(typeof(SymbolType), symbol.Type)}\n");
            }
        }
    }
}