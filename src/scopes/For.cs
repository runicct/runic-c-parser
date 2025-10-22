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
        public class For : Scope.Enter, IScope
        {
            Scope _body;
            internal Scope Body { get { return _body; } }
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
            VariableDeclaration[] _variablesDeclarations;
            public VariableDeclaration[] VariableDeclarations { get { return _variablesDeclarations; } }
            Expression? _initialization;
            public Expression? Initialization { get { return _initialization; } }
            Token _forToken;
            internal For(IScope ParentScope, Parser Context, Token ForToken)
            {
                _forToken = ForToken;
                _body = new Scope.ForScope(ParentScope, Context, this);
                _scope = _body;
            }
            public override string ToString()
            {
                return "for ( ;" + (_condition == null ? "" : _condition.ToString()) + "; " + (_increment == null ? "" : _increment.ToString()) +") {";
            }
            internal static For? ParseFor(IScope ParentScope, Parser Context, Token ForToken, TokenQueue TokenQueue)
            {
                For forLoop = new For(ParentScope, Context, ForToken);
                Function? parentFunction = forLoop.Body.GetParentFunction();
                Token? token = TokenQueue.ReadNextToken();
                if (token == null)
                {
                    Context.Error_IncompleteStatement(ForToken);
                    return null;
                }

                if (token.Value != "(")
                {
                    Context.Error_InvalidIfStatement(ForToken, token);
                    // Look up to '('
                    return null;
                }

                token = TokenQueue.ReadNextToken();
                if (token == null)
                if (token == null)
                {
                    Context.Error_IncompleteStatement(ForToken);
                    return null;
                }
                List<VariableDeclaration> variables = new List<VariableDeclaration>();
                Type? type = Type.Parse(ParentScope, Context, token, TokenQueue);
                if (token.Value != ";")
                {
                    if (type == null)
                    {
                        TokenQueue.FrontLoadToken(token);
                        forLoop._initialization = Expression.Parse(ParentScope, Context, TokenQueue, false);
                        token = TokenQueue.ReadNextToken();
                        if (token == null)
                        {
                            Context.Error_IncompleteStatement(ForToken);
                            return forLoop;
                        }
                        if (token.Value != ";")
                        {
                            Context.Error_ExpectedSemicolumn(token);
                            if (token.Value == ")")
                            {
                                // This is an error lets try to re-attach the parser
                                return forLoop;
                            }
                            if (token.Value == "{")
                            {
                                // This is an error lets assume this is the scope opening
                                TokenQueue.FrontLoadToken(token);
                                return forLoop;
                            }
                        }

                    }
                    else
                    {
                        while (true)
                        {
                            Token? variableName = TokenQueue.ReadNextToken();
                            if (variableName == null)
                            {
                                Context.Error_IncompleteStatement(ForToken);
                                return null;
                            }
                            token = TokenQueue.ReadNextToken();
                            if (token == null)
                            {
                                Context.Error_IncompleteStatement(ForToken);
                                return null;
                            }
                            switch (token.Value)
                            {
                                case "{":
                                    TokenQueue.FrontLoadToken(token);
                                    return forLoop;
                                case ";":
                                    {
                                        LocalVariable variable = new LocalVariable(new Attribute[0], type, variableName, parentFunction, parentFunction.GetNextLocalIndex());
                                        forLoop._body.AddVariable(variable);
                                        goto condition;
                                    }
                                case "=":
                                    {
                                        Token equal = token;
                                        Expression? initialization = Expression.Parse(ParentScope, Context, TokenQueue);
                                        if (Identifier.IsValid(variableName, Context.StandardRevision))
                                        {
                                            LocalVariable variable = new LocalVariable(new Attribute[0], type, variableName, parentFunction, parentFunction.GetNextLocalIndex());
                                            VariableDeclaration variableDeclaration = new VariableDeclaration(variable, equal,initialization);
                                            forLoop._body.AddVariable(variable);
                                            variables.Add(variableDeclaration);
                                        }
                                        token = TokenQueue.ReadNextToken();
                                        if (token == null) { return null; }
                                        if (token.Value == ";")
                                        {
                                            goto condition;
                                        }
                                        if (token.Value != ",")
                                        {
                                            Context.Error_ExpectedSemicolumn(token);
                                            while (token != null && token.Value != ";")
                                            {
                                                token = TokenQueue.ReadNextToken();
                                                if (token == null) { return null; }
                                                if (token.Value == "{")
                                                {
                                                    return forLoop;
                                                }
                                                if (token.Value == "}")
                                                {
                                                    return null;
                                                }
                                            }
                                            goto condition;
                                        }
                                    }
                                    break;
                                case ",":
                                    {
                                        LocalVariable variable = new LocalVariable(new Attribute[0], type, variableName, parentFunction, parentFunction.GetNextLocalIndex());
                                        forLoop._body.AddVariable(variable);
                                    }
                                    break;
                            }
                        }
                    }
                }
                condition:;
                forLoop._variablesDeclarations = variables.ToArray();
                forLoop._condition = Expression.Parse(forLoop.Body, Context, TokenQueue);
                token = TokenQueue.ReadNextToken();
                if (token == null )
                {
                    Context.Error_IncompleteStatement(ForToken);
                    return null;
                }

                if (token.Value != ";")
                {
                    if (forLoop._condition != null)
                    {
                        // No need to overwheelm the users with two errors so only report it if we have extra token
                        Context.Error_ExpectedSemicolumn(token);
                    }
                    while (token != null && token.Value != ";")
                    {
                        token = TokenQueue.ReadNextToken();
                        if (token == null) { return null; }
                        if (token.Value == "{")
                        {
                            return forLoop;
                        }
                        if (token.Value == "}")
                        {
                            return null;
                        }
                    }
                }
                forLoop._increment = Expression.Parse(forLoop.Body, Context, TokenQueue, false);
                token = TokenQueue.ReadNextToken();
                if (token == null)
                {
                    Context.Error_IncompleteStatement(ForToken);
                    return null;
                }
                if (token.Value != ")")
                {
                    if (forLoop._increment != null)
                    {
                        // No need to overwheelm the users with two errors so only report it if we have extra token
                    }
                    while (token != null && token.Value != ")")
                    {
                        token = TokenQueue.ReadNextToken();
                        if (token == null) { return null; }
                        if (token.Value == "{")
                        {
                            return forLoop;
                        }
                        if (token.Value == "}")
                        {
                            return null;
                        }
                    }
                }
                return forLoop;
            }
        }
    }
}
