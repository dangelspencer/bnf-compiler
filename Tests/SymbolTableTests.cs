using System;
using Xunit;
using BnfCompiler;

namespace Tests
{
    public class SymbolTableTests
    {
        readonly SymbolTable SymbolTable;

        public SymbolTableTests()
        {
            SymbolTable = new SymbolTable();
        }

        [Fact]
        public void SymbolTableIsNotNull() 
        {
            Assert.NotEqual(SymbolTable, null);
        }

        [Fact]
        public void CanAddSymbolToTable()
        {
            var ex = Record.Exception(() =>
            {
                var s = new Symbol("Test", SymbolType.Undefined);
                SymbolTable.AddSymbolToTable(s);
            });

            Assert.Null(ex);
        }

        [Fact]
        public void CannotAddDuplicateSymbolToTable()
        {
            var ex = Record.Exception(() =>
            {
                var s = new Symbol("Test", SymbolType.Undefined);
                SymbolTable.AddSymbolToTable(s);
                SymbolTable.AddSymbolToTable(s);
            });
            Assert.NotNull(ex);
        }
    }
}
