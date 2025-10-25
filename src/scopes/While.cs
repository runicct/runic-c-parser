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
        public class While : Scope.Enter, IScope
        {
            Token _whileToken;
            public Token WhileToken { get { return _whileToken; } }
            Scope _body;
            internal Scope Body { get { return _body; } }

            Expression _condition;
            public Expression Condition { get { return _condition; } }
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
            internal While(IScope parentScope, Parser context, Token whileToken, Expression condition)
            {
                _body = new Scope.WhileScope(parentScope, context, this);
                _scope = _body;
                _condition = condition;
                _whileToken = whileToken;
            }
            public override string ToString()
            {
                return "while (" + _condition.ToString() + ") {";
            }
#if NET6_0_OR_GREATER
            internal static While? ParseWhile(IScope ParentScope, Parser Context, Token IfToken, TokenQueue TokenQueue)
#else
            internal static While ParseWhile(IScope ParentScope, Parser Context, Token IfToken, TokenQueue TokenQueue)
#endif
            {
#if NET6_0_OR_GREATER
                Token? token = TokenQueue.ReadNextToken();
#else
                Token token = TokenQueue.ReadNextToken();
#endif
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

#if NET6_0_OR_GREATER
                Expression? condition = Expression.Parse(ParentScope, Context, TokenQueue);
#else
                Expression condition = Expression.Parse(ParentScope, Context, TokenQueue);
#endif
                if (condition == null) { condition = new Expression.Constant(new Token[] { new ImplicitToken(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, "0") }); }
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
                return new While(ParentScope, Context, IfToken, condition);
            }
        }
    }
}
