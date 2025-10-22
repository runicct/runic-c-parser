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

using System.Text;

namespace Runic.C
{
    public partial class Parser
    {
        public abstract partial class Type
        {
            public class FunctionPointerType : Type
            {
                // This is a hack, because we will already be deep in the type parsing when we encounter this
                // we return a special class and process it separatly
                internal class FunctionReturningFunctionPointer : FunctionPointerType
                {
                    public FunctionReturningFunctionPointer(FunctionPointerType ReturnType, Token? Name, FunctionParameter[] Parameters) : base(ReturnType, Name, Parameters)
                    {
                        _parameters = Parameters;
                    }
                }
                Type _returnType;
                public Type ReturnType { get { return _returnType; } }
                Token? _name;
                public Token? Name { get { return _name; } }
                FunctionParameter[] _parameters;
                public FunctionParameter[] Parameters { get { return _parameters; } }
                internal FunctionPointerType(Type ReturnType, Token? Name, FunctionParameter[] Parameters)
                {
                    _returnType = ReturnType;
                    _name = Name;
                    _parameters = Parameters;
                }

                internal static FunctionPointerType? Parse(IScope ParentScope, Parser Context, Type ReturnType, TokenQueue TokenQueue)
                {
                    FunctionParameter[] functionParameters;
                    bool declFunction = false;
                    FunctionParameter[] declFunctionParameters = null;

                    // Ok we have a function pointer that one will be a bit harder to parse
                    Token? nameOrParenthesis = TokenQueue.ReadNextToken();
                    Token? name = null;

                    if (nameOrParenthesis == null) 
                    {
                        return null;
                    }
                    if (nameOrParenthesis.Value != ")")
                    {
                        name = nameOrParenthesis;
                        nameOrParenthesis = TokenQueue.ReadNextToken();
                        if (nameOrParenthesis == null)
                        {
                            Context.Error_IncompleteDeclaration(name);
                            return null;
                        }

                        if (nameOrParenthesis.Value == "(")
                        {

                            declFunction = true;
                            List<FunctionParameter> declParametersList = new List<FunctionParameter>();
                            while (true)
                            {
                                Token? typeName = TokenQueue.ReadNextToken();
                                if (typeName == null)
                                {
                                    Context.Error_IncompleteDeclaration(name);
                                    return null;
                                }

                                Type? parameterType = Type.Parse(ParentScope, Context, typeName, TokenQueue);
                                if (parameterType == null)
                                {
                                    Context.Error_InvalidFunctionParameterType(name, typeName);
                                    parameterType = new Type.Int(typeName);
                                }

                                Token? nameOrSeparator = TokenQueue.ReadNextToken();
                                if (nameOrSeparator == null)
                                {
                                    Context.Error_IncompleteDeclaration(name);
                                    return null;
                                }

                                if (nameOrSeparator.Value == ")")
                                {
                                    declParametersList.Add(new FunctionParameter(new Attribute[0], parameterType, null, (ulong)declParametersList.Count));
                                    break;
                                }
                                if (nameOrSeparator.Value == ",")
                                {
                                    declParametersList.Add(new FunctionParameter(new Attribute[0], parameterType, null, (ulong)declParametersList.Count));
                                    continue;
                                }

                                if (!Identifier.IsValid(nameOrSeparator, Context.StandardRevision))
                                {
                                    Context.Error_InvalidIdentifier(name);
                                    if (nameOrSeparator.Value == "(") { goto resumeParameterParsing; }
                                    if (nameOrSeparator.Value == ";")
                                    {
                                        TokenQueue.FrontLoadToken(nameOrSeparator);
                                        return new FunctionReturningFunctionPointer(new FunctionPointerType(ReturnType, null, new FunctionParameter[0]), name, declParametersList.ToArray());
                                    }
                                }

                                declParametersList.Add(new FunctionParameter(new Attribute[0], parameterType, nameOrSeparator, (ulong)declParametersList.Count));
                                nameOrSeparator = TokenQueue.ReadNextToken();
                                if (nameOrSeparator == null)
                                {
                                    Context.Error_IncompleteDeclaration(name);
                                    return null;
                                }
                                if (nameOrSeparator.Value == ")") { break; }
                                if (nameOrSeparator.Value == ",") { continue; }
                                Context.Error_InvalidFunctionDefintion(name, nameOrSeparator);
                                Token? separator = nameOrSeparator;
                                while (separator.Value != ",")
                                {
                                    nameOrSeparator = TokenQueue.ReadNextToken();
                                    if (nameOrSeparator == null)
                                    {
                                        Context.Error_IncompleteDeclaration(name);
                                        return null;
                                    }
                                    if (nameOrSeparator.Value == ",") { break; }
                                    if (nameOrSeparator.Value == ")") { goto resumeParsing; }
                                    if (nameOrSeparator.Value == "(") { goto resumeParameterParsing; }
                                    if (nameOrSeparator.Value == ";")
                                    {
                                        TokenQueue.FrontLoadToken(nameOrSeparator);
                                        return new FunctionReturningFunctionPointer(new FunctionPointerType(ReturnType, null, new FunctionParameter[0]), name, declParametersList.ToArray());
                                    }
                                }
                            }
                            resumeParsing:;
                            declFunctionParameters = declParametersList.ToArray();

                            Token? closingParenthesis = TokenQueue.ReadNextToken();

                            if (closingParenthesis == null)
                            {
                                Context.Error_IncompleteDeclaration(name);
                                return null;
                            }
                            if (closingParenthesis.Value != ")") 
                            {
                                Context.Error_ExpectedParenthesis(closingParenthesis);
                                return null;
                            }
                        }
                        else if (nameOrParenthesis.Value != ")")
                        {
                            Context.Error_ExpectedParenthesis(nameOrParenthesis);
                            return null;
                        }
                    }

                    Token? token = TokenQueue.ReadNextToken();
                    if (token == null || token.Value != "(")
                    {
                        if (token == null)
                        {
                            if (name != null) { Context.Error_IncompleteDeclaration(name); }
                            else { Context.Error_IncompleteDeclaration(nameOrParenthesis); }
                        }
                        else
                        {
                            Context.Error_ExpectedParenthesis(token);
                        }
                        return null;
                    }

                    resumeParameterParsing:;
      
                    List< FunctionParameter> parameters = new List< FunctionParameter>();
                    token = TokenQueue.PeekToken();
                    if (token == null) { return null; }
                    if (token.Value == ")")
                    {
                        TokenQueue.ReadNextToken();
                        if (!Identifier.IsValid(name, Context.StandardRevision))
                        {
                            Context.Error_InvalidIdentifier(name);
                        }
                        return new FunctionPointerType(ReturnType, name, new FunctionParameter[0]);
                    }
                    ulong parameterIndex = 0;
                    while (true)
                    {
                        token = TokenQueue.ReadNextToken();
                        Type? parameterType = Type.Parse(ParentScope, Context, token, TokenQueue);
                        if (parameterType == null)
                        {
                            Context.Error_InvalidType(token);
                            parameterType = new Type.Int(token);
                        }
                        token = TokenQueue.ReadNextToken();
                        if (token == null)
                        {
                            Context.Error_IncompleteDeclaration(name);
                            return null;
                        }
                        if (token.Value == ")")
                        {
                            parameters.Add(new FunctionParameter(new Attribute[0], parameterType, null, parameterIndex));
                            parameterIndex++;
                            break;
                        }
                        if (token.Value == ",")
                        {
                            parameters.Add(new FunctionParameter(new Attribute[0], parameterType, null, parameterIndex));
                            parameterIndex++;
                            continue;
                        }
                        parameters.Add(new FunctionParameter(new Attribute[0], parameterType, token, parameterIndex));
                        parameterIndex++;
                        token = TokenQueue.ReadNextToken();
                        if (token == null) 
                        {
                            Context.Error_IncompleteDeclaration(name);
                            return null;
                        }
                        if (token.Value == "[")
                        {
                            Expression? arraySize = Expression.Parse(ParentScope, Context, TokenQueue);
                            if (arraySize == null) { /* TODO: ERROR */ }
                            token = TokenQueue.ReadNextToken();
                            if (token.Value != "]")
                            {
                                /* TODO: ERROR */
                            }
                            else
                            {
                                token = TokenQueue.ReadNextToken();
                            }
                        }
                        if (token.Value == ")")
                        {
                            break;
                        }
                        if (token.Value == ",")
                        {
                            continue;
                        }
                        Context.Error_ExpectedParenthesis(token);
                        while (token.Value != null)
                        {
                            token = TokenQueue.ReadNextToken();
                            if (token.Value == ",") { break; }
                            if (token.Value == ";" || token.Value == ")")
                            {
                                goto endOfProcessing;
                            }

                        }
                    }
                    endOfProcessing:
                    if (name != null && !Identifier.IsValid(name, Context.StandardRevision))
                    {
                        Context.Error_InvalidIdentifier(name);
                    }
                    if (declFunction)
                    {
                        FunctionPointerType returnType = new FunctionPointerType(ReturnType, null, parameters.ToArray());
                        return new FunctionReturningFunctionPointer(returnType, name, declFunctionParameters);
                    }
                    else
                    {
                        return new FunctionPointerType(ReturnType, name, parameters.ToArray());
                    }
                }

                public override string ToString()
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(_returnType.ToString());
                    builder.Append("(*");
                    if (_name != null) { builder.Append(_name); }
                    builder.Append(")(");
                    for (int n = 0; n < _parameters.Length; n++)
                    {
                        if (n != 0) { builder.Append(", "); }
                        builder.Append(_parameters[n].ToString());
                    }
                    builder.Append(")");
                    return builder.ToString();
                }
            }

        }
    }
}
