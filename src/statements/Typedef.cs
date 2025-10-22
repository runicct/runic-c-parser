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
        public class Typedef : Statement
        {
            Type _type;
            public Type Type { get { return _type; } }
            Token? _name;
            public Token? Name { get { return _name; } }
            internal Typedef(Type Type, Token? Name)
            {
                _type = Type;
                _name = Name;

            }
            internal static Typedef[]? Parse(IScope ParentScope, Parser Context, Token TypedefToken, TokenQueue TokenQueue)
            {
                List<Typedef> results = new List<Typedef>();
                Token? typeToken = TokenQueue.ReadNextToken();
                if (typeToken == null)
                {
                    Context.Error_MissingTypedefType(TypedefToken);
                    Token? token = TokenQueue.ReadNextToken();
                    while (token != null && token.Value != ";") { token = TokenQueue.ReadNextToken(); }
                    return null;
                }
                Type? type = Type.Parse(ParentScope, Context, typeToken, TokenQueue);
                if (type == null)
                {
                    Context.Error_InvalidTypedefType(TypedefToken, typeToken);
                    Token? token = TokenQueue.ReadNextToken();
                    while (token != null && token.Value != ";") { token = TokenQueue.ReadNextToken(); }
                    return null;
                }
                {
                    Type.StructOrUnion? structOrUnion = type as Type.StructOrUnion;
                    if (structOrUnion != null)
                    {
                        while (structOrUnion.Declaration != null && structOrUnion.Declaration != structOrUnion)
                        {
                            type = structOrUnion.Declaration;
                            structOrUnion = type as Type.StructOrUnion;
                        }
                    }
                }
                Type.FunctionPointerType functionPointer = type as Type.FunctionPointerType;

                if (functionPointer != null)
                {
                    results.Add(new Typedef(type, functionPointer.Name));

                    if (functionPointer.Name == null)
                    {
                        Context.Error_AnonymousFunctionPointerTypeInTypedef(TypedefToken, functionPointer);
                    }
                    Token? endStatement = TokenQueue.ReadNextToken();
                    if (endStatement == null)
                    {
                        Context.Error_ExpectedSemicolumn(functionPointer.Name);
                    }
                    else if (endStatement.Value != ";")
                    {
                        Context.Error_ExpectedSemicolumn(endStatement);
                        Token? token = TokenQueue.ReadNextToken();
                        while (token != null && token.Value != ";") { token = TokenQueue.ReadNextToken(); }
                    }
                    return results.ToArray();
                }
                else
                {

                    Token? name = TokenQueue.ReadNextToken();

                    results.Add(new Typedef(type, name));

                    if (name == null)
                    {
                        Context.Error_MissingTypedefName(TypedefToken);
                        Token? token = TokenQueue.ReadNextToken();
                        while (token != null && token.Value != ";") { token = TokenQueue.ReadNextToken(); }
                        return null;
                    }

                    Token? endStatement = TokenQueue.ReadNextToken();
                    if (endStatement == null)
                    {
                        Context.Error_ExpectedSemicolumn(name);
                    }
                    else if (endStatement.Value == ",")
                    {
                        while (true)
                        {
                            type = type.Strip();
                            Token? nextToken = TokenQueue.ReadNextToken();
                            while (true)
                            {
                                if (nextToken == null)
                                {
                                    Context.Error_ExpectedSemicolumn(endStatement);
                                    return results.ToArray();
                                }

                                if (nextToken.Value == "*")
                                {
                                    type = type.MakePointer(nextToken);
                                    nextToken = TokenQueue.ReadNextToken();
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (!Identifier.IsValid(nextToken, Context.StandardRevision))
                            {
                                Context.Error_InvalidTypedefType(TypedefToken, nextToken);
                            }
                            else
                            {
                                name = nextToken;
                                results.Add(new Typedef(type, name));
                            }

                            nextToken = TokenQueue.ReadNextToken();
                            if (nextToken == null) { Context.Error_ExpectedSemicolumn(endStatement); return results.ToArray(); }
                            if (nextToken.Value == ",") { continue; }
                            if (nextToken.Value == ";") { return results.ToArray(); }
                            Context.Error_ExpectedSemicolumn(nextToken);
                            Token? token = TokenQueue.ReadNextToken();
                            while (token != null && token.Value != ";") { token = TokenQueue.ReadNextToken(); }
                            return results.ToArray();
                        }
                    }
                    else if (endStatement.Value != ";")
                    {
                        Context.Error_ExpectedSemicolumn(endStatement);
                        Token? token = TokenQueue.ReadNextToken();
                        while (token != null && token.Value != ";") { token = TokenQueue.ReadNextToken(); }
                    }
                    return results.ToArray();
                }
            }

            public override string ToString()
            {
                Type.FunctionPointerType functionPointer = _type as Type.FunctionPointerType;
                if (functionPointer != null) { return "typedef " + _type + ";"; }
                return "typedef " + _type + " " + _name + ";";
            }
        }
    }
}
