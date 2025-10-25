/*
 * MIT License
 * 
 * Copyright (c) 2025 Runic Compiler Toolkit Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace Runic.C
{
    public partial class Parser
    {
        // Even if it may look odd an UnscopedFor (i.e. for (X;X;X) Something ;) is still
        // an implicit scope because the variable declared in the for are only scoped to
        // the following statement. This make unscopedFor special and hard to handle
        public class UnscopedFor : Scope.Enter, IScope
        {
            Scope.UnscopedForScope _body;
            internal Scope.UnscopedForScope Body { get { return _body; } }
#if NET6_0_OR_GREATER
            Expression? _condition;
            public Expression? Condition { get { return _condition; } }
            Expression? _increment;
            public Expression? Increment { get { return _increment; } }
            public IScope? ParentScope { get { return _body.ParentScope; } }
            public Type? ResolveType(string Name) { return _body.ResolveType(Name); }
            public Variable? ResolveVariable(string Name) { return _body.ResolveVariable(Name); }
            public Function? ResolveFunction(string Name) { return _body.ResolveFunction(Name); }
            public Type.Enum.Member? ResolveEnumMember(string Name) { return _body.ResolveEnumMember(Name); }
            public IScope? GetBreakContinueScope() { return _body.GetBreakContinueScope(); }
#else
            Expression _condition;
            public Expression Condition { get { return _condition; } }
            Expression _increment;
            public Expression Increment { get { return _increment; } }
            public IScope ParentScope { get { return _body.ParentScope; } }
            public Type ResolveType(string Name) { return _body.ResolveType(Name); }
            public Variable ResolveVariable(string Name) { return _body.ResolveVariable(Name); }
            public Function ResolveFunction(string Name) { return _body.ResolveFunction(Name); }
            public Type.Enum.Member ResolveEnumMember(string Name) { return _body.ResolveEnumMember(Name); }
            public IScope GetBreakContinueScope() { return _body.GetBreakContinueScope(); }
#endif
            VariableDeclaration[] _variablesDeclarations;
            public VariableDeclaration[] VariableDeclarations { get { return _variablesDeclarations; } }
            Token _forToken;
            internal UnscopedFor(IScope ParentScope, Parser Context, Token ForToken, VariableDeclaration[] variableDeclarations, Expression condition, Expression increment)
            {
                _forToken = ForToken;
                _body = new Scope.UnscopedForScope(ParentScope, Context, this);
                _scope = _body;
                _variablesDeclarations = variableDeclarations;
                _condition = condition;
                _increment = increment;
            }
            public override string ToString()
            {
                return "for ( ;" + (_condition == null ? "" : _condition.ToString()) + "; " + (_increment == null ? "" : _increment.ToString()) + ")";
            }
        }
    }
}