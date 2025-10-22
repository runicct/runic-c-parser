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
        public class DoWhile : Scope.Enter, IScope
        {
            Token _token;
            public Token Token { get { return _token; } }
            Scope _body;
            internal Scope Body { get { return _body; } }

            Expression _condition;
            public Expression Condition { get { return _condition; } }
            public IScope? ParentScope { get { return _body.ParentScope; } }
            public Type? ResolveType(string Name) { return _body.ResolveType(Name); }
            public Variable? ResolveVariable(string Name) { return _body.ResolveVariable(Name); }
            public Function? ResolveFunction(string Name) { return _body.ResolveFunction(Name); }
            public Type.Enum.Member? ResolveEnumMember(string Name) { return _body.ResolveEnumMember(Name); }
            public IScope? GetBreakContinueScope() { return _body.GetBreakContinueScope(); }
            internal DoWhile(IScope ParentScope, Parser Context, Token DoToken)
            {
                _body = new Scope.DoWhileScope(ParentScope, Context, this);
                _scope = _body;
                _condition = Condition;
            }
            internal static Scope.ExitDoWhile? ParseExitDoWhile(IScope ParentScope, Token? PreviousToken, Parser Context, DoWhile DoWhile, TokenQueue TokenQueue)
            {
                Token? token = TokenQueue.ReadNextToken();
                Token? previousToken = PreviousToken;

                if (token == null)
                {
                    Context.Error_ExpectedWhile(previousToken);
                    return null;
                }
                if (token.Value != "while")
                {
                    Context.Error_ExpectedWhile(token);
                    return null;
                }
                Token whileToken = token;
                previousToken = token;
                token = TokenQueue.ReadNextToken();
                if (token == null)
                {
                    Context.Error_ExpectedCondition(DoWhile._token, token);
                    return null;
                }
                if (token.Value != "(")
                {
                    Context.Error_ExpectedCondition(DoWhile._token, token);
                    while (!Context.ReattachToken(token)) { token = TokenQueue.ReadNextToken(); }
                    return null;
                }
                previousToken = token;
                Expression? condition = Expression.Parse(ParentScope, Context, TokenQueue);
                if (condition == null)
                {
                    Context.Error_ExpectedCondition(DoWhile._token, previousToken);
                    return null;
                }
                token = TokenQueue.ReadNextToken();
                if (token == null)
                {
                    Context.Error_ExpectedCondition(DoWhile._token, previousToken);
                    return null;
                }
                if (token.Value != ")")
                {
                    Context.Error_ExpectedCondition(DoWhile._token, token);
                    while (token != null && (token.Value != ")" || token.Value != "}" || token.Value != ";")) { token = TokenQueue.ReadNextToken(); }
                    return null;
                }
                previousToken = token;
                token = TokenQueue.ReadNextToken();
                if (token == null)
                {
                    Context.Error_ExpectedSemicolumn(previousToken);
                    return null;
                }
                if (token.Value != ";")
                {
                    Context.Error_ExpectedSemicolumn(token);
                    while (!Context.ReattachToken(token)) { token = TokenQueue.ReadNextToken(); }
                    return null;
                }
                return new Scope.ExitDoWhile(whileToken, DoWhile.Body, DoWhile, condition);
            }

            public override string ToString()
            {
                return "do {";
            }
        }
    }
}
