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
        public class Switch : Scope.Enter, IScope
        {
            Scope _body;
            internal Scope Body { get { return _body; } }

            Expression _value;
            public Expression Value { get { return _value; } }
            public IScope? ParentScope { get { return _body.ParentScope; } }
            public Type? ResolveType(string Name) { return _body.ResolveType(Name); }
            public Variable? ResolveVariable(string Name) { return _body.ResolveVariable(Name); }
            public Function? ResolveFunction(string Name) { return _body.ResolveFunction(Name); }
            public Type.Enum.Member? ResolveEnumMember(string Name) { return _body.ResolveEnumMember(Name); }
            public IScope? GetBreakContinueScope() { return _body.GetBreakContinueScope(); }
            internal Switch(IScope ParentScope, Parser Context, Token SwitchToken, Expression Value)
            {
                _body = new Scope.SwitchScope(ParentScope, Context, this);
                _scope = _body;
                _value = Value;
            }
            public override string ToString()
            {
                return "switch (" + _value.ToString() + ") {";
            }
            internal static Switch? ParseSwitch(IScope ParentScope, Parser Context, Token IfToken, TokenQueue TokenQueue)
            {
                Token? token = TokenQueue.ReadNextToken();
                if (token == null)
                {
                    Context.Error_IncompleteStatement(IfToken);
                    return null;
                }

                if (token.Value != "(")
                {
                    Context.Error_InvalidIfStatement(IfToken, token);
                    // Look up to '('
                    return null;
                }

                Expression? value = Expression.Parse(ParentScope, Context, TokenQueue);

                token = TokenQueue.ReadNextToken();
                if (token == null)
                {
                    Context.Error_IncompleteStatement(IfToken);
                    return null;
                }
                if (token.Value != ")")
                {
                    Context.Error_InvalidIfStatement(IfToken, token);
                    while (token != null && token.Value != ")" && token.Value != ";" && token.Value != "{")
                    {
                        token = TokenQueue.ReadNextToken();
                    }
                    if (token == null)
                    {
                        return null;
                    }
                    if (token.Value == "{")
                    {
                        TokenQueue.FrontLoadToken(token);
                    }
                }
                return new Switch(ParentScope, Context, IfToken, value);
            }
        }
    }
}
