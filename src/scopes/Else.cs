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
        public class Else : Scope.Enter, IScope
        {
            Scope _body;
            internal Scope Body { get { return _body; } }
#if NET6_0_OR_GREATER
            public IScope? ParentScope { get { return _body.ParentScope; } }
            public Type? ResolveType(string Name) { return _body.ResolveType(Name); }
            public Variable? ResolveVariable(string Name) { return _body.ResolveVariable(Name); }
            public Function? ResolveFunction(string Name) { return _body.ResolveFunction(Name); }
            public Type.Enum.Member? ResolveEnumMember(string Name) { return _body.ResolveEnumMember(Name); }
            public IScope? GetBreakContinueScope() { return _body.GetBreakContinueScope(); }
#else
            public IScope ParentScope { get { return _body.ParentScope; } }
            public Type ResolveType(string Name) { return _body.ResolveType(Name); }
            public Variable ResolveVariable(string Name) { return _body.ResolveVariable(Name); }
            public Function ResolveFunction(string Name) { return _body.ResolveFunction(Name); }
            public Type.Enum.Member ResolveEnumMember(string Name) { return _body.ResolveEnumMember(Name); }
            public IScope GetBreakContinueScope() { return _body.GetBreakContinueScope(); }
#endif
            Token _elseToken;
            public Token Keyword { get { return _elseToken; } }
            internal Else(IScope ParentScope, Parser Context, Token ElseToken)
            {
                _elseToken = ElseToken;
                _body = new Scope.ElseScope(ParentScope, Context, this);
                _scope = _body;
            }
            public override string ToString()
            {
                return "else {";
            }
        }
    }
}
